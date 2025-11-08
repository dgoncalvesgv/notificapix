import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { authApi } from "../../lib/api/client";
import { useAuthStore } from "../../store/auth";
import { useToast } from "../../context/ToastContext";
import { Link, useNavigate } from "react-router-dom";

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(1)
});

type FormValues = z.infer<typeof schema>;

export const LoginPage = () => {
  const { register, handleSubmit, formState } = useForm<FormValues>({
    resolver: zodResolver(schema)
  });
  const setSession = useAuthStore((state) => state.setSession);
  const toast = useToast();
  const navigate = useNavigate();

  const onSubmit = handleSubmit(async (values) => {
    const { data } = await authApi.login(values);
    if (data.success && data.data) {
      setSession(data.data);
      toast.push("Bem-vindo de volta!", "success");
      navigate("/app/overview");
    } else {
      toast.push(data.error ?? "Falha no login", "error");
    }
  });

  return (
    <form className="space-y-4" onSubmit={onSubmit}>
      <h1 className="text-xl font-semibold">Entrar</h1>
      <label className="block">
        <span className="text-sm text-slate-500">E-mail</span>
        <input className="input" type="email" {...register("email")} />
        {formState.errors.email && <p className="text-xs text-red-500">{formState.errors.email.message}</p>}
      </label>
      <label className="block">
        <span className="text-sm text-slate-500">Senha</span>
        <input className="input" type="password" {...register("password")} />
        {formState.errors.password && <p className="text-xs text-red-500">{formState.errors.password.message}</p>}
      </label>
      <button type="submit" className="btn-primary w-full">
        Acessar dashboard
      </button>
      <div className="flex justify-between text-xs text-slate-500">
        <Link to="/forgot" className="text-primary-600">
          Esqueci minha senha
        </Link>
        <Link to="/register" className="text-primary-600">
          Criar conta
        </Link>
      </div>
    </form>
  );
};
