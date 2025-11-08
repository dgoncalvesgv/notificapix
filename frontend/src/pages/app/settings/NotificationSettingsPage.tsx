import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { settingsApi } from "../../../lib/api/client";
import { useToast } from "../../../context/ToastContext";

type FormValues = {
  emailsCsv: string;
  webhookUrl?: string;
  webhookSecret?: string;
  enabled: boolean;
};

export const NotificationSettingsPage = () => {
  const { register, handleSubmit, reset } = useForm<FormValues>({
    defaultValues: { emailsCsv: "", enabled: true }
  });
  const toast = useToast();

  useEffect(() => {
    settingsApi.getNotifications().then((response) => {
      const payload = response.data.data;
      if (payload) {
        reset({
          emailsCsv: payload.emails.join(", "),
          webhookSecret: payload.webhookSecret,
          webhookUrl: payload.webhookUrl,
          enabled: payload.enabled
        });
      }
    });
  }, [reset]);

  const onSubmit = handleSubmit(async (values) => {
    await settingsApi.updateNotifications({
      emails: values.emailsCsv.split(",").map((email) => email.trim()).filter(Boolean),
      webhookUrl: values.webhookUrl,
      webhookSecret: values.webhookSecret,
      enabled: values.enabled
    });
    toast.push("Configuração salva", "success");
  });

  return (
    <form className="space-y-4" onSubmit={onSubmit}>
      <h1 className="text-2xl font-semibold">Notificações</h1>
      <label className="block">
        <span className="text-sm text-slate-500">E-mails (separados por vírgula)</span>
        <input className="input" {...register("emailsCsv")} placeholder="alerts@empresa.com" />
      </label>
      <label className="block">
        <span className="text-sm">Webhook URL</span>
        <input className="input" {...register("webhookUrl")} />
      </label>
      <label className="block">
        <span className="text-sm">Webhook Secret</span>
        <input className="input" {...register("webhookSecret")} />
      </label>
      <label className="flex items-center gap-2 text-sm">
        <input type="checkbox" {...register("enabled")} />
        Habilitar envio
      </label>
      <button className="btn-primary">Salvar</button>
    </form>
  );
};
