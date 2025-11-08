import { useEffect, useState } from "react";
import { billingApi, orgApi } from "../../../lib/api/client";
import type { OrganizationDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

const plans = [
  { name: "Starter", quota: "100 notificações", price: "R$ 99/mês" },
  { name: "Pro", quota: "1.000 notificações", price: "R$ 399/mês" },
  { name: "Business", quota: "Ilimitado", price: "Custom" }
];

export const BillingPage = () => {
  const [organization, setOrganization] = useState<OrganizationDto>();
  const toast = useToast();

  useEffect(() => {
    orgApi.current().then((res) => setOrganization(res.data.data));
  }, []);

  const managePortal = async () => {
    const { data } = await billingApi.portal();
    toast.push(`Abrindo portal: ${data.data?.url}`, "info");
  };

  return (
    <div className="space-y-6">
      <header className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Billing</h1>
          <p className="text-sm text-slate-500">Plano atual: {organization?.plan}</p>
        </div>
        <button className="btn-primary" onClick={managePortal}>
          Portal Stripe
        </button>
      </header>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {plans.map((plan) => (
          <div key={plan.name} className="border rounded-xl p-4 bg-white space-y-2">
            <h3 className="font-semibold">{plan.name}</h3>
            <p className="text-2xl">{plan.price}</p>
            <p className="text-sm text-slate-500">{plan.quota}</p>
            <button
              disabled={organization?.plan === plan.name}
              className="btn-primary w-full disabled:bg-slate-200 disabled:text-slate-500"
              onClick={async () => {
                await billingApi.checkout({ plan: plan.name });
                toast.push("Sessão de checkout criada", "success");
              }}
            >
              {organization?.plan === plan.name ? "Atual" : "Selecionar"}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};
