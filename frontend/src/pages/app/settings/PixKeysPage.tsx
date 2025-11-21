import { FormEvent, useEffect, useState } from "react";
import { pixApi } from "../../../lib/api/client";
import type { PixReceiverDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

const keyTypes = ["CPF", "CNPJ", "Email", "Celular", "Aleatória"];

export const PixKeysPage = () => {
  const [keys, setKeys] = useState<PixReceiverDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ label: "", keyType: keyTypes[0], keyValue: "" });
  const [saving, setSaving] = useState(false);
  const [selecting, setSelecting] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const toast = useToast();

  const loadKeys = () => {
    setLoading(true);
    pixApi
      .listKeys()
      .then((response) => {
        const payload = response.data.data;
        if (payload) {
          setKeys(payload.options ?? []);
        }
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadKeys();
  }, []);

  const createKey = async (event: FormEvent) => {
    event.preventDefault();
    if (!form.label || !form.keyValue) {
      toast.push("Preencha todos os campos", "error");
      return;
    }
    setSaving(true);
    try {
      const response = await pixApi.createKey(form);
      if (response.data.data) {
        toast.push("Chave cadastrada com sucesso", "success");
        setForm({ label: "", keyType: keyTypes[0], keyValue: "" });
        loadKeys();
      }
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível cadastrar a chave.";
      toast.push(message, "error");
    } finally {
      setSaving(false);
    }
  };

  const setDefault = async (id: string) => {
    setSelecting(id);
    try {
      await pixApi.selectKey({ pixKeyId: id });
      toast.push("Chave definida como padrão", "success");
      loadKeys();
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível atualizar a chave padrão.";
      toast.push(message, "error");
    } finally {
      setSelecting(null);
    }
  };

  const deleteKey = async (id: string) => {
    if (!window.confirm("Deseja realmente excluir esta chave Pix?")) {
      return;
    }
    setDeletingId(id);
    try {
      await pixApi.deleteKey(id);
      toast.push("Chave excluída.", "success");
      loadKeys();
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível excluir a chave.";
      toast.push(message, "error");
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold">Chaves Pix</h1>
        <p className="text-sm text-slate-500">Cadastre as chaves Pix utilizadas para gerar QR codes.</p>
      </header>

      <form onSubmit={createKey} className="grid grid-cols-1 md:grid-cols-3 gap-4 bg-white border rounded-2xl p-4">
        <label className="text-sm space-y-1">
          <span>Descrição</span>
          <input className="input" value={form.label} onChange={(e) => setForm((prev) => ({ ...prev, label: e.target.value }))} placeholder="Ex.: Chave principal" />
        </label>
        <label className="text-sm space-y-1">
          <span>Tipo</span>
          <select className="input" value={form.keyType} onChange={(e) => setForm((prev) => ({ ...prev, keyType: e.target.value }))}>
            {keyTypes.map((type) => (
              <option key={type}>{type}</option>
            ))}
          </select>
        </label>
        <label className="text-sm space-y-1">
          <span>Chave</span>
          <input className="input" value={form.keyValue} onChange={(e) => setForm((prev) => ({ ...prev, keyValue: e.target.value }))} placeholder="Informe a chave Pix" />
        </label>
        <div className="md:col-span-3 flex justify-end">
          <button className="btn-primary" type="submit" disabled={saving}>
            {saving ? "Salvando..." : "Adicionar chave"}
          </button>
        </div>
      </form>

      <div className="rounded-2xl border border-slate-200 bg-white">
        {loading ? (
          <div className="p-6 text-sm text-slate-500">Carregando chaves...</div>
        ) : keys.length === 0 ? (
          <div className="p-6 text-sm text-slate-500">Nenhuma chave cadastrada ainda.</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs uppercase text-slate-500">
                <th className="px-4 py-2">Descrição</th>
                <th>Tipo</th>
                <th>Chave</th>
                <th className="text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              {keys.map((pixKey) => (
                <tr key={pixKey.id} className="border-t border-slate-100">
                  <td className="px-4 py-2">
                    {pixKey.label}
                    {pixKey.isDefault && <span className="ml-2 rounded-full bg-green-50 text-green-700 text-xs px-2 py-0.5">Padrão</span>}
                  </td>
                  <td>{pixKey.keyType}</td>
                  <td className="font-mono text-xs break-all">{pixKey.keyValue}</td>
                 <td className="text-right">
                   <button
                     className="text-primary-600 text-sm font-semibold disabled:text-slate-400"
                     disabled={pixKey.isDefault || selecting === pixKey.id}
                     onClick={() => setDefault(pixKey.id)}
                   >
                     {selecting === pixKey.id ? "Salvando..." : pixKey.isDefault ? "Padrão" : "Tornar padrão"}
                   </button>
                    <button
                      className="ml-3 text-sm text-red-600 disabled:text-slate-400"
                      disabled={deletingId === pixKey.id || selecting === pixKey.id}
                      onClick={() => deleteKey(pixKey.id)}
                    >
                      {deletingId === pixKey.id ? "Excluindo..." : "Excluir"}
                    </button>
                 </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};
