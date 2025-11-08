import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { authApi } from "../../lib/api/client";
import { useAuthStore } from "../../store/auth";
import { useNavigate, Link } from "react-router-dom";
import { useToast } from "../../context/ToastContext";

const schema = z.object({
  name: z.string().min(1),
  organizationName: z.string().min(1),
  email: z.string().email(),
  password: z.string().min(8)
});

type FormValues = z.infer<typeof schema>;

export const RegisterPage = () => {
  const { register, handleSubmit, formState } = useForm<FormValues>({
    resolver: zodResolver(schema)
  });
  const setSession = useAuthStore((state) => state.setSession);
  const navigate = useNavigate();
  const toast = useToast();

  const onSubmit = handleSubmit(async (values) => {
    const { data } = await authApi.register(values);
    if (data.success && data.data) {
      setSession(data.data);
      toast.push("Conta criada! Configure o Stripe e notificações.", "success");
      navigate("/app/overview");
    } else {
      toast.push(data.error ?? "Erro ao cadastrar", "error");
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <h1 className="text-xl font-semibold">Criar conta</h1>
      {["name", "organizationName", "email", "password"].map((field) => (
        <label key={field} className="block capitalize">
          <span className="text-sm text-slate-500">{field}</span>
          <input
            className="input"
            type={field === "password" ? "password" : field === "email" ? "email" : "text"}
            {...register(field as keyof FormValues)}
          />
          {formState.errors[field as keyof FormValues] && (
            <p className="text-xs text-red-500">{formState.errors[field as keyof FormValues]?.message as string}</p>
          )}
        </label>
      ))}
      <button type="submit" className="btn-primary w-full">
        Registrar
      </button>
      <p className="text-xs text-slate-500">
        Já tem conta?{" "}
        <Link to="/login" className="text-primary-600">
          Entrar
        </Link>
      </p>
    </form>
  );
};
