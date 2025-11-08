import { useEffect, useState } from "react";
import { apiKeysApi } from "../../../lib/api/client";
import type { ApiKeyDto } from "../../../lib/api/types";
import { useForm } from "react-hook-form";
import { CopyToClipboard } from "../../../components/CopyToClipboard";
import { useToast } from "../../../context/ToastContext";

export const ApiKeysPage = () => {
  const [keys, setKeys] = useState<ApiKeyDto[]>([]);
  const [lastSecret, setLastSecret] = useState<string>();
  const { register, handleSubmit, reset } = useForm<{ name: string }>();
  const toast = useToast();

  const load = () => {
    apiKeysApi.list().then((res) => setKeys(res.data.data ?? []));
  };

  useEffect(() => {
    load();
  }, []);

  const create = handleSubmit(async (values) => {
    const { data } = await apiKeysApi.create(values);
    if (data.data) {
      setLastSecret(data.data.secret);
      toast.push("API Key criada. Copie o segredo!", "info");
      reset();
      load();
    }
  });

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-semibold">API Keys</h1>
      {lastSecret && (
        <div className="bg-amber-50 border border-amber-200 text-amber-700 px-4 py-2 rounded">
          Segredo novo: <code>{lastSecret}</code>
        </div>
      )}
      <form onSubmit={create} className="flex gap-2">
        <input className="input" placeholder="Nome interno" {...register("name")} />
        <button className="btn-primary">Criar</button>
      </form>
      <table className="w-full text-sm bg-white rounded-xl border">
        <thead>
          <tr className="text-left text-xs uppercase text-slate-500">
            <th className="px-4 py-2">Nome</th>
            <th>Status</th>
            <th>Criado</th>
            <th>Ação</th>
          </tr>
        </thead>
        <tbody>
          {keys.map((key) => (
            <tr key={key.id} className="border-t border-slate-100">
              <td className="px-4 py-2">{key.name}</td>
              <td>{key.isActive ? "Ativa" : "Revogada"}</td>
              <td>{new Date(key.createdAt).toLocaleString()}</td>
              <td>
                <CopyToClipboard value={key.id} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
