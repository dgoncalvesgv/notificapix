import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { useToast } from "../../../context/ToastContext";
import { orgApi } from "../../../lib/api/client";
import { useAuthStore } from "../../../store/auth";

type FormValues = {
  name: string;
  billingEmail: string;
};

export const AccountSettingsPage = () => {
  const organization = useAuthStore((state) => state.organization);
  const updateOrganization = useAuthStore((state) => state.updateOrganization);
  const toast = useToast();

  const {
    register,
    handleSubmit,
    reset,
    formState: { isSubmitting, isDirty }
  } = useForm<FormValues>({
    defaultValues: { name: organization?.name ?? "", billingEmail: organization?.billingEmail ?? "" }
  });

  useEffect(() => {
    if (organization) {
      reset({ name: organization.name, billingEmail: organization.billingEmail });
    }
  }, [organization, reset]);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      const response = await orgApi.current();
      const payload = response.data.data;
      if (!cancelled && payload) {
        updateOrganization(payload);
        reset({ name: payload.name, billingEmail: payload.billingEmail });
      }
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [reset, updateOrganization]);

  const onSubmit = handleSubmit(async (values) => {
    const response = await orgApi.update(values);
    const payload = response.data.data;
    if (payload) {
      updateOrganization(payload);
      reset({ name: payload.name, billingEmail: payload.billingEmail });
      toast.push("Cadastro atualizado com sucesso.", "success");
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-6 max-w-2xl">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Dados do cadastro</h1>
        <p className="text-sm text-slate-500">Atualize o nome exibido e o e-mail utilizado para faturamento e notificações.</p>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm space-y-4">
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-600">Nome da organização</span>
          <input className="input" {...register("name")} placeholder="Minha Empresa LTDA" />
        </label>
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-600">E-mail de cobrança</span>
          <input className="input" type="email" {...register("billingEmail")} placeholder="financeiro@empresa.com" />
        </label>
        <p className="text-xs text-slate-500">
          Esse e-mail será usado para contato financeiro e envio de alertas administrativos. Certifique-se de que alguém o monitore.
        </p>
        <div className="flex justify-end">
          <button className="btn-primary" type="submit" disabled={isSubmitting || !isDirty}>
            {isSubmitting ? "Salvando..." : "Salvar alterações"}
          </button>
        </div>
      </div>
    </form>
  );
};
