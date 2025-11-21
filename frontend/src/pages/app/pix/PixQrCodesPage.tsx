import { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import QRCode from "qrcode";
import { pixApi } from "../../../lib/api/client";
import type { PixReceiverDto, PixStaticQrCodeDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";
import { useAuthStore } from "../../../store/auth";

type SortColumn = "description" | "receiver" | "createdAt";
type SortState = {
  sortBy: SortColumn;
  direction: "asc" | "desc";
};

const EyeIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" className="h-4 w-4">
    <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7S1 12 1 12Z" />
    <circle cx="12" cy="12" r="3" />
  </svg>
);

const TrashIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" className="h-4 w-4">
    <path d="M3 6h18" />
    <path d="M8 6V4h8v2" />
    <path d="M10 11v6" />
    <path d="M14 11v6" />
    <path d="M5 6l1 14c.1 1 1 2 2 2h8c1 0 1.9-1 2-2l1-14" />
  </svg>
);

export const PixQrCodesPage = () => {
  const [qrCodes, setQrCodes] = useState<PixStaticQrCodeDto[]>([]);
  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [amountInput, setAmountInput] = useState("");
  const [descriptionInput, setDescriptionInput] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [selectedQr, setSelectedQr] = useState<PixStaticQrCodeDto | null>(null);
  const [selectedQrImage, setSelectedQrImage] = useState<string | null>(null);
  const toast = useToast();
  const [loadingList, setLoadingList] = useState(false);
  const [pixKeys, setPixKeys] = useState<PixReceiverDto[]>([]);
  const [keysLoading, setKeysLoading] = useState(false);
  const [selectedKeyId, setSelectedKeyId] = useState<string | null>(null);
  const [filtersInput, setFiltersInput] = useState({ description: "", fromDate: "", toDate: "" });
  const [appliedFilters, setAppliedFilters] = useState(filtersInput);
  const [sortState, setSortState] = useState<SortState>({ sortBy: "createdAt", direction: "desc" });
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [pendingDeleteQr, setPendingDeleteQr] = useState<PixStaticQrCodeDto | null>(null);
  const organization = useAuthStore((state) => state.organization);
  const pixQrCodesLimit = organization?.pixQrCodesLimit ?? 0;
  const qrLimitReached = pixQrCodesLimit > 0 && qrCodes.length >= pixQrCodesLimit;
  const qrUsageText = pixQrCodesLimit > 0 ? `${Math.min(qrCodes.length, pixQrCodesLimit)}/${pixQrCodesLimit}` : null;

  const loadQrCodes = useCallback(() => {
    setLoadingList(true);
    pixApi
      .listQrCodes({
        description: appliedFilters.description || undefined,
        createdFrom: appliedFilters.fromDate || undefined,
        createdTo: appliedFilters.toDate || undefined,
        sortBy: sortState.sortBy,
        sortDirection: sortState.direction
      })
      .then((response) => setQrCodes(response.data.data ?? []))
      .finally(() => setLoadingList(false));
  }, [appliedFilters, sortState]);

  const loadKeys = () => {
    setKeysLoading(true);
    pixApi
      .listKeys()
      .then((response) => {
        const payload = response.data.data;
        if (payload) {
          setPixKeys(payload.options ?? []);
          setSelectedKeyId(payload.selected?.id ?? payload.options?.find((k) => k.isDefault)?.id ?? payload.options?.[0]?.id ?? null);
        }
      })
      .finally(() => setKeysLoading(false));
  };

  useEffect(() => {
    loadQrCodes();
  }, [loadQrCodes]);

  useEffect(() => {
    loadKeys();
  }, []);

  useEffect(() => {
    if (!selectedQr) {
      setSelectedQrImage(null);
      return;
    }
    QRCode.toDataURL(selectedQr.payload, { width: 256, errorCorrectionLevel: "M" }).then(setSelectedQrImage);
  }, [selectedQr]);

  const formattedCodes = useMemo(
    () =>
      qrCodes.map((code) => ({
        ...code,
        formattedAmount: new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(code.amount)
      })),
    [qrCodes]
  );

  const handleGenerate = async () => {
    const amount = parseFloat(amountInput);
    if (isNaN(amount) || amount <= 0) {
      return;
    }
    if (pixKeys.length === 0) {
      toast.push("Cadastre uma chave Pix antes de gerar QR codes.", "error");
      return;
    }
    if (qrLimitReached) {
      toast.push("Você atingiu o limite de QR codes do seu plano.", "error");
      setShowGenerateModal(false);
      return;
    }
    setIsGenerating(true);
    try {
      await pixApi.createQrCode({
        amount,
        pixKeyId: selectedKeyId ?? undefined,
        description: descriptionInput.trim() || undefined
      });
      setAmountInput("");
      setDescriptionInput("");
      setShowGenerateModal(false);
      toast.push("QR Code gerado com sucesso.", "success");
      loadQrCodes();
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível gerar o QR Code.";
      toast.push(message, "error");
    } finally {
      setIsGenerating(false);
    }
  };

  const handleApplyFilters = () => {
    setAppliedFilters({ ...filtersInput });
  };

  const handleResetFilters = () => {
    const cleared = { description: "", fromDate: "", toDate: "" };
    setFiltersInput(cleared);
    setAppliedFilters(cleared);
  };

  const toggleSort = (column: SortColumn) => {
    setSortState((prev) => {
      if (prev.sortBy === column) {
        return { sortBy: column, direction: prev.direction === "asc" ? "desc" : "asc" };
      }
      return { sortBy: column, direction: column === "createdAt" ? "desc" : "asc" };
    });
  };

  const sortIndicator = (column: SortColumn) => {
    if (sortState.sortBy !== column) {
      return "⇅";
    }
    return sortState.direction === "asc" ? "↑" : "↓";
  };

  const handleDeleteQrCode = async () => {
    if (!pendingDeleteQr) return;
    setDeletingId(pendingDeleteQr.id);
    try {
      await pixApi.deleteQrCode(pendingDeleteQr.id);
      setQrCodes((prev) => prev.filter((code) => code.id !== pendingDeleteQr.id));
      toast.push("QR code excluído.", "success");
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível excluir o QR code.";
      toast.push(message, "error");
    } finally {
      setDeletingId(null);
      setPendingDeleteQr(null);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">QR Codes PIX</h1>
          <p className="text-sm text-slate-500">Gere e visualize códigos estáticos.</p>
          {qrUsageText && <p className="text-xs text-slate-500 mt-1">QR codes armazenados: {qrUsageText}</p>}
        </div>
        <button className="btn-primary disabled:bg-slate-200 disabled:text-slate-500" onClick={() => setShowGenerateModal(true)} disabled={qrLimitReached}>
          Gerar QR Code
        </button>
      </div>
      {qrLimitReached && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-xs text-amber-800">
          Limite de QR codes atingido. Exclua registros antigos ou{" "}
          <Link to="/app/billing" className="font-semibold text-primary-700 underline">
            acesse Plano para fazer upgrade
          </Link>{" "}
          e liberar mais geração.
        </div>
      )}
      {!keysLoading && pixKeys.length === 0 && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">
          Nenhuma chave Pix cadastrada. Acesse Configurações → Chaves Pix para adicionar uma antes de gerar QR codes.
        </div>
      )}
      <div className="rounded-xl border border-slate-200 bg-white p-4 dark:bg-slate-800 dark:border-slate-700">
        <div className="grid gap-4 md:grid-cols-4">
          <label className="block text-sm space-y-1 md:col-span-2">
            <span>Descrição</span>
            <input
              className="input"
              value={filtersInput.description}
              onChange={(e) => setFiltersInput((prev) => ({ ...prev, description: e.target.value }))}
              placeholder="Buscar descrição"
            />
          </label>
          <label className="block text-sm space-y-1">
            <span>Criado a partir de</span>
            <input
              className="input"
              type="date"
              value={filtersInput.fromDate}
              onChange={(e) => setFiltersInput((prev) => ({ ...prev, fromDate: e.target.value }))}
            />
          </label>
          <label className="block text-sm space-y-1">
            <span>Criado até</span>
            <input
              className="input"
              type="date"
              value={filtersInput.toDate}
              onChange={(e) => setFiltersInput((prev) => ({ ...prev, toDate: e.target.value }))}
            />
          </label>
        </div>
        <div className="mt-4 flex flex-wrap gap-2 justify-end">
          <button className="border border-slate-200 rounded px-4 py-2 text-sm" onClick={handleResetFilters}>
            Limpar
          </button>
          <button className="btn-primary text-sm" onClick={handleApplyFilters}>
            Filtrar
          </button>
        </div>
      </div>
      <div className="rounded-xl border border-slate-200 bg-white dark:bg-slate-800 dark:border-slate-700">
        {loadingList ? (
          <div className="p-6 text-sm text-slate-500">Carregando QR codes...</div>
        ) : formattedCodes.length === 0 ? (
          <div className="p-6 text-sm text-slate-500">Nenhum QR code foi criado ainda.</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs uppercase text-slate-500">
                <th className="px-4 py-2">
                  <button className="flex items-center gap-1 font-semibold" onClick={() => toggleSort("description")}>
                    Descrição <span className="text-[10px]">{sortIndicator("description")}</span>
                  </button>
                </th>
                <th>Valor</th>
                <th>
                  <button className="flex items-center gap-1 font-semibold" onClick={() => toggleSort("receiver")}>
                    Recebedor <span className="text-[10px]">{sortIndicator("receiver")}</span>
                  </button>
                </th>
                <th>
                  <button className="flex items-center gap-1 font-semibold" onClick={() => toggleSort("createdAt")}>
                    Criado em <span className="text-[10px]">{sortIndicator("createdAt")}</span>
                  </button>
                </th>
                <th className="text-right pr-5">Ações</th>
              </tr>
            </thead>
            <tbody>
              {formattedCodes.map((code) => (
                <tr key={code.id} className="border-t border-slate-100">
                  <td className="px-4 py-2">
                    <div className="font-semibold">{code.description || "Sem descrição"}</div>
                    <div className="text-xs text-slate-500 font-mono">TxID: {code.txId}</div>
                  </td>
                  <td>{code.formattedAmount}</td>
                  <td>{code.receiverLabel}</td>
                  <td>{new Date(code.createdAt).toLocaleString("pt-BR")}</td>
                  <td className="text-right pr-5">
                    <div className="inline-flex items-center gap-3">
                      <button
                        className="inline-flex h-8 w-8 items-center justify-center rounded-full text-primary-600 hover:bg-primary-50"
                        onClick={() => setSelectedQr(code)}
                        title="Ver QR Code"
                      >
                        <EyeIcon />
                        <span className="sr-only">Ver QR Code</span>
                      </button>
                      <button
                        className="inline-flex h-8 w-8 items-center justify-center rounded-full text-red-600 hover:bg-red-50 disabled:opacity-50 disabled:hover:bg-transparent"
                        onClick={() => setPendingDeleteQr(code)}
                        disabled={deletingId === code.id}
                        title={deletingId === code.id ? "Excluindo..." : "Excluir QR Code"}
                      >
                        <TrashIcon />
                        <span className="sr-only">Excluir QR Code</span>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showGenerateModal && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4 border border-slate-200 dark:border-slate-700">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">Novo QR Code</h2>
              <button className="text-slate-500" onClick={() => setShowGenerateModal(false)}>
                ✕
              </button>
            </div>
            {qrLimitReached && (
              <p className="text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded px-3 py-2">
                Limite de QR codes do plano atingido. Exclua um QR code existente ou{" "}
                <Link to="/app/billing" className="font-semibold text-primary-700 underline">
                  faça upgrade no Plano
                </Link>{" "}
                antes de gerar um novo.
              </p>
            )}
            {keysLoading ? (
              <p className="text-sm text-slate-500">Carregando chaves Pix...</p>
            ) : pixKeys.length === 0 ? (
              <p className="text-sm text-slate-500">Nenhuma chave Pix cadastrada. Acesse Configurações → Chaves Pix para adicionar uma.</p>
            ) : (
              <label className="block text-sm space-y-1">
                <span>Chave Pix</span>
                <select className="input" value={selectedKeyId ?? ""} onChange={(e) => setSelectedKeyId(e.target.value)}>
                  {pixKeys.map((key) => (
                    <option key={key.id} value={key.id}>
                      {key.label} ({key.keyType})
                    </option>
                  ))}
                </select>
              </label>
            )}
            <label className="block text-sm space-y-1">
              <span>Valor a receber</span>
              <input
                className="input"
                type="number"
                min="0"
                step="0.01"
                value={amountInput}
                onChange={(e) => setAmountInput(e.target.value)}
                placeholder="100.00"
              />
            </label>
            <label className="block text-sm space-y-1">
              <span>Descrição (opcional)</span>
              <input
                className="input"
                maxLength={250}
                value={descriptionInput}
                onChange={(e) => setDescriptionInput(e.target.value)}
                placeholder="Descrição interna"
              />
            </label>
            <div className="flex justify-end gap-2">
              <button className="border border-slate-200 rounded px-4 py-2" onClick={() => setShowGenerateModal(false)} disabled={isGenerating}>
                Cancelar
              </button>
              <button className="btn-primary" onClick={handleGenerate} disabled={isGenerating || !amountInput || pixKeys.length === 0 || qrLimitReached}>
                {isGenerating ? "Gerando..." : "Gerar"}
              </button>
            </div>
          </div>
        </div>
      )}

      {selectedQr && selectedQrImage && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4 text-center border border-slate-200 dark:border-slate-700">
            <img src={selectedQrImage} alt="QR Code PIX" className="mx-auto" />
            <p className="text-sm text-slate-500">
              Escaneie para pagar {new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(selectedQr.amount)}
            </p>
            {selectedQr.description && <p className="text-sm font-medium">{selectedQr.description}</p>}
            <p className="text-xs text-slate-500 font-mono">TxID: {selectedQr.txId}</p>
            <button className="text-primary-600 text-sm font-semibold" onClick={() => setSelectedQr(null)}>
              Fechar
            </button>
          </div>
        </div>
      )}
      {pendingDeleteQr && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4 border border-slate-200 dark:border-slate-700">
            <h2 className="text-lg font-semibold text-slate-900 dark:text-white">Remover QR Code</h2>
            <p className="text-sm text-slate-600 dark:text-slate-300">
              Deseja excluir o QR code com descrição <strong>{pendingDeleteQr.description || "Sem descrição"}</strong>?
            </p>
            <div className="flex justify-end gap-2">
              <button
                className="px-4 py-2 rounded-lg border border-slate-200 text-slate-600 dark:text-slate-200"
                onClick={() => !deletingId && setPendingDeleteQr(null)}
                disabled={!!deletingId}
              >
                Cancelar
              </button>
              <button className="btn-primary" onClick={handleDeleteQrCode} disabled={!!deletingId}>
                {deletingId ? "Removendo..." : "Excluir"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
