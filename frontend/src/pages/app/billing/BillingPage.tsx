import { useEffect, useState, type FormEvent } from "react";
import { Elements, PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { billingApi, orgApi } from "../../../lib/api/client";
import type { OrganizationDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

const plans = [
  { name: "Starter", quota: "100 notificações", price: "R$ 99/mês" },
  { name: "Pro", quota: "1.000 notificações", price: "R$ 399/mês" },
  { name: "Business", quota: "Ilimitado", price: "Custom" }
];

const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY;
const stripePromise = publishableKey ? loadStripe(publishableKey) : null;

export const BillingPage = () => {
  const [organization, setOrganization] = useState<OrganizationDto>();
  const [selectedPlan, setSelectedPlan] = useState<string | null>(null);
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [subscriptionId, setSubscriptionId] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [loadingPlan, setLoadingPlan] = useState<string | null>(null);
  const toast = useToast();

  const fetchOrganization = () => orgApi.current().then((res) => setOrganization(res.data.data));

  useEffect(() => {
    fetchOrganization();
  }, []);

  const openCheckout = async (planName: string) => {
    if (!stripePromise) {
      toast.push("Configure VITE_STRIPE_PUBLISHABLE_KEY para habilitar o checkout.", "error");
      return;
    }

    setLoadingPlan(planName);
    try {
      const { data } = await billingApi.checkout({ plan: planName });
      if (!data.success || !data.data?.clientSecret) {
        throw new Error(data.error ?? "Não foi possível iniciar o checkout.");
      }
      setSelectedPlan(planName);
      setClientSecret(data.data.clientSecret);
      setSubscriptionId(data.data.subscriptionId);
      setIsModalOpen(true);
    } catch (error) {
      toast.push("Não foi possível iniciar o checkout.", "error");
    } finally {
      setLoadingPlan(null);
    }
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setClientSecret(null);
    setSubscriptionId(null);
    setSelectedPlan(null);
  };

  const handleSuccess = () => {
    toast.push("Pagamento confirmado! Atualizaremos o plano em instantes.", "success");
    closeModal();
    fetchOrganization();
  };

  const managePortal = async () => {
    const { data } = await billingApi.portal();
    const url = data.data?.url;
    if (url) {
      window.open(url, "_blank");
    }
    toast.push(url ? "Abrindo portal da Stripe." : "Não foi possível abrir o portal.", url ? "info" : "error");
  };

  return (
    <div className="space-y-6 relative">
      <header className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Billing</h1>
          <p className="text-sm text-slate-500">Plano atual: {organization?.plan}</p>
        </div>
        <button className="btn-primary" onClick={managePortal}>
          Portal Stripe
        </button>
      </header>
      {!stripePromise && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">
          Configure <code>VITE_STRIPE_PUBLISHABLE_KEY</code> para habilitar o checkout embutido.
        </div>
      )}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {plans.map((plan) => (
          <div key={plan.name} className="border rounded-xl p-4 bg-white space-y-2">
            <h3 className="font-semibold">{plan.name}</h3>
            <p className="text-2xl">{plan.price}</p>
            <p className="text-sm text-slate-500">{plan.quota}</p>
            <button
              disabled={organization?.plan === plan.name || loadingPlan === plan.name}
              className="btn-primary w-full disabled:bg-slate-200 disabled:text-slate-500"
              onClick={() => openCheckout(plan.name)}
            >
              {loadingPlan === plan.name
                ? "Preparando..."
                : organization?.plan === plan.name
                  ? "Atual"
                  : "Selecionar"}
            </button>
          </div>
        ))}
      </div>
      {isModalOpen && clientSecret && stripePromise && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg p-6 space-y-4">
            <div className="flex items-start justify-between">
              <div>
                <h2 className="text-xl font-semibold">Confirmar assinatura</h2>
                <p className="text-sm text-slate-500">
                  Plano selecionado: <span className="font-medium">{selectedPlan}</span>
                </p>
                {subscriptionId && (
                  <p className="text-xs text-slate-400 mt-1">Assinatura: {subscriptionId}</p>
                )}
              </div>
              <button className="text-slate-500 hover:text-slate-700" onClick={closeModal}>
                ✕
              </button>
            </div>
            <Elements stripe={stripePromise} options={{ clientSecret, appearance: { theme: "stripe" } }}>
              <CheckoutForm onCancel={closeModal} onSuccess={handleSuccess} />
            </Elements>
          </div>
        </div>
      )}
    </div>
  );
};

const CheckoutForm = ({
  onCancel,
  onSuccess
}: {
  onCancel: () => void;
  onSuccess: () => void;
}) => {
  const stripe = useStripe();
  const elements = useElements();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!stripe || !elements) return;

    setIsSubmitting(true);
    setMessage(null);

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {},
      redirect: "if_required"
    });

    if (error) {
      setMessage(error.message ?? "Não foi possível confirmar o pagamento.");
    } else {
      onSuccess();
    }
    setIsSubmitting(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <PaymentElement options={{ layout: "tabs" }} />
      {message && <p className="text-sm text-red-600">{message}</p>}
      <div className="flex justify-end gap-2">
        <button
          type="button"
          className="px-4 py-2 rounded-lg border border-slate-200 text-slate-600"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancelar
        </button>
        <button
          type="submit"
          className="btn-primary disabled:opacity-60"
          disabled={!stripe || !elements || isSubmitting}
        >
          {isSubmitting ? "Processando..." : "Confirmar pagamento"}
        </button>
      </div>
    </form>
  );
};
