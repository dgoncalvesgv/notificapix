import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { teamApi } from "../../../lib/api/client";
import type { TeamMemberDto } from "../../../lib/api/types";
import { useForm } from "react-hook-form";
import { useToast } from "../../../context/ToastContext";
import { useAuthStore } from "../../../store/auth";

export const TeamPage = () => {
  const [members, setMembers] = useState<TeamMemberDto[]>([]);
  const { register, handleSubmit, reset } = useForm<{ email: string; role: string }>({ defaultValues: { role: "OrgMember" } });
  const toast = useToast();
  const organization = useAuthStore((state) => state.organization);
  const teamLimit = organization?.teamMembersLimit ?? 0;
  const reachedTeamLimit = teamLimit === 0 || (teamLimit > 0 && members.length >= teamLimit);

  const load = () => {
    teamApi.list().then((response) => setMembers(response.data.data ?? []));
  };

  useEffect(() => {
    load();
  }, []);

  const invite = handleSubmit(async (values) => {
    if (reachedTeamLimit) {
      toast.push("Limite de integrantes atingido para o seu plano.", "error");
      return;
    }
    try {
      await teamApi.invite(values);
      toast.push("Convite enviado", "success");
      reset({ email: "", role: "OrgMember" });
      load();
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível enviar o convite.";
      toast.push(message, "error");
    }
  });

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">Time</h1>
      <form onSubmit={invite} className="grid grid-cols-1 md:grid-cols-3 gap-2 bg-white rounded-xl border p-4 dark:bg-slate-800 dark:border-slate-700">
        <input className="input" placeholder="email@empresa.com" {...register("email")} disabled={reachedTeamLimit} />
        <select className="input" {...register("role")} disabled={reachedTeamLimit}>
          <option value="OrgMember">Membro</option>
          <option value="OrgAdmin">Admin</option>
        </select>
        <button className="btn-primary disabled:bg-slate-200 disabled:text-slate-500" disabled={reachedTeamLimit}>
          Convidar
        </button>
        {reachedTeamLimit && (
          <div className="md:col-span-3 rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-xs text-amber-800">
            {teamLimit === 0
              ? "Seu plano atual não permite convidar novos usuários."
              : `Limite de integrantes atingido para o plano (${teamLimit}). `}
            <Link to="/app/billing" className="font-semibold text-primary-700 underline">
              Faça upgrade em Plano para liberar novos convites.
            </Link>
          </div>
        )}
      </form>
      <table className="w-full text-sm bg-white rounded-xl border dark:bg-slate-800 dark:border-slate-700">
        <thead>
          <tr className="text-left text-xs uppercase text-slate-500">
            <th className="px-4 py-2">Email</th>
            <th>Perfil</th>
            <th>Entrada</th>
          </tr>
        </thead>
        <tbody>
          {members.map((member) => (
            <tr key={member.membershipId} className="border-t border-slate-100">
              <td className="px-4 py-2">{member.email}</td>
              <td>{member.role}</td>
              <td>{new Date(member.joinedAt).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
