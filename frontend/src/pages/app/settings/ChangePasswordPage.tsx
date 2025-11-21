import { useForm } from "react-hook-form";
import { authApi } from "../../../lib/api/client";
import { useToast } from "../../../context/ToastContext";

type FormValues = {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
};

export const ChangePasswordPage = () => {
  const toast = useToast();
  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<FormValues>({
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" }
  });

  const onSubmit = handleSubmit(async (values) => {
    await authApi.changePassword({ currentPassword: values.currentPassword, newPassword: values.newPassword });
    toast.push("Senha atualizada com sucesso.", "success");
    reset();
  });

  const passwordMismatch = errors.confirmPassword?.message;

  return (
    <form onSubmit={onSubmit} className="space-y-6 max-w-xl">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Segurança</h1>
        <p className="text-sm text-slate-500">Atualize sua senha para manter sua conta protegida.</p>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm space-y-4 dark:bg-slate-800 dark:border-slate-700">
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-600">Senha atual</span>
          <input className="input" type="password" autoComplete="current-password" {...register("currentPassword", { required: true })} />
        </label>
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-600">Nova senha</span>
          <input
            className="input"
            type="password"
            autoComplete="new-password"
            {...register("newPassword", { required: true, minLength: 8 })}
          />
          <span className="text-xs text-slate-500">Use pelo menos 8 caracteres, misturando letras, números e símbolos.</span>
        </label>
        <label className="block space-y-1">
          <span className="text-sm font-medium text-slate-600">Confirmar nova senha</span>
          <input
            className="input"
            type="password"
            autoComplete="new-password"
            {...register("confirmPassword", {
              required: true,
              validate: (value) => value === watch("newPassword") || "As senhas não coincidem."
            })}
          />
          {passwordMismatch && <span className="text-xs text-rose-500">{passwordMismatch}</span>}
        </label>
        <div className="flex justify-end">
          <button className="btn-primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Salvando..." : "Alterar senha"}
          </button>
        </div>
      </div>
    </form>
  );
};
