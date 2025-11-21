import { useEffect, useMemo, useState } from "react";
import QRCode from "qrcode";
import { pixApi } from "../../../lib/api/client";
import type { PixReceiverDto, PixStaticQrCodeDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

export const PixQrCodesPage = () => {
  const [qrCodes, setQrCodes] = useState<PixStaticQrCodeDto[]>([]);
  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [amountInput, setAmountInput] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [selectedQr, setSelectedQr] = useState<PixStaticQrCodeDto | null>(null);
  const [selectedQrImage, setSelectedQrImage] = useState<string | null>(null);
  const toast = useToast();
  const [loadingList, setLoadingList] = useState(false);
  const [pixKeys, setPixKeys] = useState<PixReceiverDto[]>([]);
  const [keysLoading, setKeysLoading] = useState(false);
  const [selectedKeyId, setSelectedKeyId] = useState<string | null>(null);

  const loadQrCodes = () => {
    setLoadingList(true);
    pixApi
      .listQrCodes()
      .then((response) => setQrCodes(response.data.data ?? []))
      .finally(() => setLoadingList(false));
  };

  const loadKeys = () => {
    setKeysLoading(true);
    pixApi
      .listKeys()
      .then((response) => {
        const payload = response.data.data;
        if (payload) {
          setPixKeys(payload.options ?? []);
          setSelectedKeyId(payload.selected?.id ?? payload.options?.find((k) => k.isDefault)?.id ?? null);
        }
      })
      .finally(() => setKeysLoading(false));
  };

  useEffect(() => {
    loadQrCodes();
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
    setIsGenerating(true);
    try {
      const response = await pixApi.createQrCode({ amount, pixKeyId: selectedKeyId ?? undefined });
      const payload = response.data.data;
      if (payload) {
        setQrCodes((prev) => [payload, ...prev]);
        setAmountInput("");
        setShowGenerateModal(false);
        toast.push("QR Code gerado com sucesso.", "success");
      }
    } catch (error: any) {
      const message = error?.response?.data?.error ?? "Não foi possível gerar o QR Code.";
      toast.push(message, "error");
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">QR Codes PIX</h1>
          <p className="text-sm text-slate-500">Gere e visualize códigos estáticos.</p>
        </div>
        <button className="btn-primary" onClick={() => setShowGenerateModal(true)}>
          Gerar QR Code
        </button>
      </div>
      {!keysLoading && pixKeys.length === 0 && (
        <div className="rounded-lg border border-amber-300 bg-amber-50 px-4 py-2 text-sm text-amber-800">
          Nenhuma chave Pix cadastrada. Acesse Configurações → Chaves Pix para adicionar uma antes de gerar QR codes.
        </div>
      )}
      <div className="rounded-xl border border-slate-200 bg-white">
        {loadingList ? (
          <div className="p-6 text-sm text-slate-500">Carregando QR codes...</div>
        ) : formattedCodes.length === 0 ? (
          <div className="p-6 text-sm text-slate-500">Nenhum QR code foi criado ainda.</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs uppercase text-slate-500">
                <th className="px-4 py-2">TXID</th>
                <th>Valor</th>
                <th>Recebedor</th>
                <th>Criado em</th>
                <th className="text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              {formattedCodes.map((code) => (
                <tr key={code.id} className="border-t border-slate-100">
                  <td className="px-4 py-2 font-mono text-xs">{code.id}</td>
                  <td>{code.formattedAmount}</td>
                  <td>{code.receiverLabel}</td>
                  <td>{new Date(code.createdAt).toLocaleString("pt-BR")}</td>
                  <td className="text-right">
                    <button className="text-primary-600 text-sm font-semibold" onClick={() => setSelectedQr(code)}>
                      Ver QR Code
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showGenerateModal && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">Novo QR Code</h2>
              <button className="text-slate-500" onClick={() => setShowGenerateModal(false)}>
                ✕
              </button>
            </div>
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
            <div className="flex justify-end gap-2">
              <button className="border border-slate-200 rounded px-4 py-2" onClick={() => setShowGenerateModal(false)} disabled={isGenerating}>
                Cancelar
              </button>
              <button className="btn-primary" onClick={handleGenerate} disabled={isGenerating || !amountInput || pixKeys.length === 0}>
                {isGenerating ? "Gerando..." : "Gerar"}
              </button>
            </div>
          </div>
        </div>
      )}

      {selectedQr && selectedQrImage && (
        <div className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4 text-center">
            <img src={selectedQrImage} alt="QR Code PIX" className="mx-auto" />
            <p className="text-sm text-slate-500">Escaneie para pagar {new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(selectedQr.amount)}</p>
            <button className="text-primary-600 text-sm font-semibold" onClick={() => setSelectedQr(null)}>
              Fechar
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
