import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { pixApi } from "../../../lib/api/client";
import type { PixReceiverDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";
import { useAuthStore } from "../../../store/auth";

const keyTypes = ["CPF", "CNPJ", "Email", "Celular", "Aleatória"];

export const PixKeysPage = () => {
  const [keys, setKeys] = useState<PixReceiverDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ label: "", keyType: keyTypes[0], keyValue: "" });
  const [saving, setSaving] = useState(false);
  const [selecting, setSelecting] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<PixReceiverDto | null>(null);
  const toast = useToast();
  const organization = useAuthStore((state) => state.organization);
  const pixKeysLimit = organization?.pixKeysLimit ?? 0;
  const keysLimitReached = pixKeysLimit > 0 && keys.length >= pixKeysLimit;
  const keysUsageText = pixKeysLimit > 0 ? `${Math.min(keys.length, pixKeysLimit)}/${pixKeysLimit}` : null;

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
    if (keysLimitReached) {
      toast.push("Você atingiu o limite de chaves Pix do seu plano.", "error");
      return;
    }
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

  const deleteKey = async () => {
    if (!pendingDelete) return;
    setDeletingId(pendingDelete.id);
    try {
      await pixApi.deleteKey(pendingDelete.id);
      toast.push("Chave excluída.", "success");
      loadKeys();
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível excluir a chave.";
      toast.push(message, "error");
    } finally {
      setDeletingId(null);
      setPendingDelete(null);
    }
  };

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-2xl font-semibold">Chaves Pix</h1>
        <p className="text-sm text-slate-500">Cadastre as chaves Pix utilizadas para gerar QR codes.</p>
        {keysUsageText && (
          <p className="text-xs text-slate-500 mt-1">Chaves utilizadas: {keysUsageText}</p>
        )}
      </header>

      {keysLimitReached && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-xs text-amber-800">
          Limite de chaves Pix atingido. Exclua uma chave existente ou{" "}
          <Link to="/app/billing" className="font-semibold text-primary-700 underline">
            faça upgrade no Plano
          </Link>{" "}
          para liberar novos cadastros.
        </div>
      )}

      <form onSubmit={createKey} className="grid grid-cols-1 md:grid-cols-3 gap-4 bg-white border rounded-2xl p-4 dark:bg-slate-800 dark:border-slate-700">
        <label className="text-sm space-y-1">
          <span>Descrição</span>
          <input className="input" value={form.label} onChange={(e) => setForm((prev) => ({ ...prev, label: e.target.value }))} placeholder="Ex.: Chave principal" disabled={keysLimitReached} />
        </label>
        <label className="text-sm space-y-1">
          <span>Tipo</span>
          <select className="input" value={form.keyType} onChange={(e) => setForm((prev) => ({ ...prev, keyType: e.target.value }))} disabled={keysLimitReached}>
            {keyTypes.map((type) => (
              <option key={type}>{type}</option>
            ))}
          </select>
        </label>
        <label className="text-sm space-y-1">
          <span>Chave</span>
          <input className="input" value={form.keyValue} onChange={(e) => setForm((prev) => ({ ...prev, keyValue: e.target.value }))} placeholder="Informe a chave Pix" disabled={keysLimitReached} />
        </label>
        <div className="md:col-span-3 flex justify-end">
          <button className="btn-primary" type="submit" disabled={saving || keysLimitReached}>
            {saving ? "Salvando..." : "Adicionar chave"}
          </button>
        </div>
      </form>

      <div className="rounded-2xl border border-slate-200 bg-white dark:bg-slate-800 dark:border-slate-700">
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
                <th className="text-right pr-6">Ações</th>
              </tr>
            </thead>
            <tbody>
              {keys.map((pixKey) => (
                <tr key={pixKey.id} className="border-t border-slate-100 dark:border-slate-700">
                  <td className="px-4 py-2">
                    {pixKey.label}
                    {pixKey.isDefault && <span className="ml-2 rounded-full bg-green-50 text-green-700 text-xs px-2 py-0.5">Padrão</span>}
                  </td>
                  <td>{pixKey.keyType}</td>
                  <td className="font-mono text-xs break-all">{pixKey.keyValue}</td>
                  <td className="text-right pr-4">
                    <div className="inline-flex items-center gap-3">
                      <button
                        className="inline-flex h-8 w-8 items-center justify-center rounded-full text-primary-600 hover:bg-primary-50 disabled:text-slate-400 disabled:hover:bg-transparent"
                        disabled={pixKey.isDefault || selecting === pixKey.id}
                        onClick={() => setDefault(pixKey.id)}
                        title="Definir como padrão"
                      >
                        {selecting === pixKey.id ? <SpinnerIcon /> : <StarIcon filled={pixKey.isDefault} />}
                        <span className="sr-only">
                          {selecting === pixKey.id ? "Salvando..." : pixKey.isDefault ? "Padrão" : "Tornar padrão"}
                        </span>
                      </button>
                      <button
                        className="inline-flex h-8 w-8 items-center justify-center rounded-full text-red-600 hover:bg-red-50 disabled:text-slate-400 disabled:hover:bg-transparent"
                        disabled={deletingId === pixKey.id || selecting === pixKey.id}
                        onClick={() => setPendingDelete(pixKey)}
                        title="Excluir chave"
                      >
                        {deletingId === pixKey.id ? <SpinnerIcon /> : <TrashIcon />}
                        <span className="sr-only">{deletingId === pixKey.id ? "Excluindo..." : "Excluir"}</span>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      {pendingDelete && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4 border border-slate-200 dark:border-slate-700">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Remover chave Pix</h2>
            <p className="text-sm text-slate-600 dark:text-slate-300">
              Tem certeza de que deseja excluir a chave <strong>{pendingDelete.label}</strong>?
            </p>
            <div className="flex justify-end gap-2">
              <button
                className="px-4 py-2 rounded-lg border border-slate-200 text-slate-600 dark:text-slate-200"
                onClick={() => !deletingId && setPendingDelete(null)}
                disabled={!!deletingId}
              >
                Cancelar
              </button>
              <button
                className="btn-primary"
                onClick={deleteKey}
                disabled={!!deletingId}
              >
                {deletingId ? "Removendo..." : "Excluir"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
const StarIcon = ({ filled }: { filled: boolean }) => (
  <svg viewBox="0 0 24 24" className={`h-4 w-4 ${filled ? "fill-current" : "fill-none"}`} stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
  </svg>
);

const TrashIcon = () => (
  <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round">
    <path d="M3 6h18" />
    <path d="M8 6V4h8v2" />
    <path d="M10 11v6" />
    <path d="M14 11v6" />
    <path d="M5 6l1 14c.1 1 1 2 2 2h8c1 0 1.9-1 2-2l1-14" />
  </svg>
);

const SpinnerIcon = () => (
  <svg viewBox="0 0 24 24" className="h-4 w-4 animate-spin" fill="none">
    <circle cx="12" cy="12" r="9" stroke="currentColor" strokeOpacity="0.3" strokeWidth="2" />
    <path d="M21 12a9 9 0 0 0-9-9" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
  </svg>
);
