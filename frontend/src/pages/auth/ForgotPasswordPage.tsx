import { useForm } from "react-hook-form";
import { authApi } from "../../lib/api/client";
import { useToast } from "../../context/ToastContext";
import { Link } from "react-router-dom";

type FormValues = { email: string };

export const ForgotPasswordPage = () => {
  const { register, handleSubmit } = useForm<FormValues>();
  const toast = useToast();

  const onSubmit = handleSubmit(async (values) => {
    await authApi.forgot(values);
    toast.push("Se existir, enviamos instruções.", "success");
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <h1 className="text-xl font-semibold">Recuperar senha</h1>
      <label className="block">
        <span className="text-sm text-slate-500">E-mail</span>
        <input className="input" type="email" {...register("email")} />
      </label>
      <button type="submit" className="btn-primary w-full">
        Enviar link
      </button>
      <Link to="/login" className="text-xs text-primary-600">
        Voltar para login
      </Link>
    </form>
  );
};
