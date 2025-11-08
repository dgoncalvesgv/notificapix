import { useForm } from "react-hook-form";
import { useToast } from "../../context/ToastContext";

type FormValues = { email: string; token: string; newPassword: string };

export const ResetPasswordPage = () => {
  const { register, handleSubmit } = useForm<FormValues>();
  const toast = useToast();

  const onSubmit = handleSubmit(() => {
    toast.push("Senha redefinida (mock)", "success");
  });

  return (
    <form onSubmit={onSubmit} className="space-y-3">
      <h1 className="text-xl font-semibold">Definir nova senha</h1>
      {["email", "token", "newPassword"].map((field) => (
        <label key={field} className="block">
          <span className="text-sm text-slate-500 capitalize">{field}</span>
          <input
            className="input"
            type={field === "newPassword" ? "password" : "text"}
            {...register(field as keyof FormValues)}
          />
        </label>
      ))}
      <button className="btn-primary w-full">Atualizar</button>
    </form>
  );
};
