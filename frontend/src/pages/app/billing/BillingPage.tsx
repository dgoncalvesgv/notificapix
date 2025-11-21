import { useEffect, useState, type FormEvent } from "react";
import { Elements, PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { billingApi, orgApi } from "../../../lib/api/client";
import type { OrganizationDto, PlanInfoDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

const fallbackPlans: PlanInfoDto[] = [
  {
    plan: "Starter",
    displayName: "Grátis",
    priceText: "R$ 0/mês",
    monthlyTransactions: 30,
    teamMembersLimit: 1,
    bankAccountsLimit: 1,
    pixKeysLimit: 1,
    pixQrCodesLimit: 20
  },
  {
    plan: "Pro",
    displayName: "Pro",
    priceText: "R$ 399/mês",
    monthlyTransactions: 1000,
    teamMembersLimit: 0,
    bankAccountsLimit: 0,
    pixKeysLimit: 0,
    pixQrCodesLimit: 0
  },
  {
    plan: "Business",
    displayName: "Business",
    priceText: "Custom",
    monthlyTransactions: 1000000,
    teamMembersLimit: 0,
    bankAccountsLimit: 0,
    pixKeysLimit: 0,
    pixQrCodesLimit: 0
  }
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
  const [plans, setPlans] = useState<PlanInfoDto[]>([]);
  const toast = useToast();

  const fetchOrganization = () => orgApi.current().then((res) => setOrganization(res.data.data));

  useEffect(() => {
    fetchOrganization();
    billingApi
      .plans()
      .then((res) => setPlans(res.data.data ?? fallbackPlans))
      .catch(() => setPlans(fallbackPlans));
  }, []);

  const planList = plans.length ? plans : fallbackPlans;

  const openCheckout = async (plan: PlanInfoDto) => {
    if (!stripePromise) {
      toast.push("Configure VITE_STRIPE_PUBLISHABLE_KEY para habilitar o checkout.", "error");
      return;
    }

    setLoadingPlan(plan.plan);
    try {
      const { data } = await billingApi.checkout({ plan: plan.plan });
      if (!data.success || !data.data?.clientSecret) {
        throw new Error(data.error ?? "Não foi possível iniciar o checkout.");
      }
      setSelectedPlan(plan.displayName);
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

  return (
    <div className="space-y-6 relative">
      <header>
        <div>
          <h1 className="text-2xl font-semibold">Billing</h1>
          <p className="text-sm text-slate-500">
            Plano atual: {organization?.planDisplayName ?? "—"}
          </p>
        </div>
      </header>
      {!stripePromise && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">
          Configure <code>VITE_STRIPE_PUBLISHABLE_KEY</code> para habilitar o checkout embutido.
        </div>
      )}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {planList.map((plan) => (
          <div key={plan.plan} className="border rounded-xl p-4 bg-white dark:bg-slate-800 dark:border-slate-700 space-y-4">
            <div>
              <h3 className="font-semibold text-xl">{plan.displayName}</h3>
              <p className="text-2xl mt-1">{plan.priceText}</p>
              <p className="text-sm text-slate-500 dark:text-slate-300">
                {plan.plan === "Starter"
                  ? "Comece a receber Pix com o essencial para testar o produto."
                  : plan.plan === "Pro"
                    ? "Automatize fluxos com limites altos e suporte prioritário."
                    : "Plano sob medida para empresas com demandas críticas e SLA dedicado."}
              </p>
            </div>
            <ul className="space-y-2 text-sm text-slate-600 dark:text-slate-200">
              <li>• {plan.monthlyTransactions <= 0 || plan.monthlyTransactions >= 1_000_000 ? "Transações ilimitadas" : `${plan.monthlyTransactions.toLocaleString("pt-BR")} transações/mês`}</li>
              <li>• {plan.teamMembersLimit === 0 ? "Time ilimitado" : `${plan.teamMembersLimit} membro(s) do time`}</li>
              <li>• {plan.bankAccountsLimit === 0 ? "Contas bancárias ilimitadas" : `${plan.bankAccountsLimit} conta(s) bancária(s)`}</li>
              <li>• {plan.pixKeysLimit === 0 ? "Chaves Pix ilimitadas" : `${plan.pixKeysLimit} chave(s) Pix`}</li>
              <li>• {plan.pixQrCodesLimit === 0 ? "QR Codes ilimitados" : `${plan.pixQrCodesLimit} QR Codes ativos`}</li>
            </ul>
            <button
              disabled={organization?.plan === plan.plan || loadingPlan === plan.plan}
              className="btn-primary w-full disabled:bg-slate-200 disabled:text-slate-500"
              onClick={() => openCheckout(plan)}
            >
              {loadingPlan === plan.plan
                ? "Preparando..."
                : organization?.plan === plan.plan
                  ? "Atual"
                  : "Contratar"}
            </button>
          </div>
        ))}
      </div>
      {isModalOpen && clientSecret && stripePromise && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl w-full max-w-lg p-6 space-y-4 border border-slate-200 dark:border-slate-800">
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
