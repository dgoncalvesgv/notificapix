import { useEffect, useState } from "react";
import { bankApi } from "../../../lib/api/client";
import type { BankConnectionDto, BankIntegrationStatusDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

const apiBanks = [
  "Itaú Unibanco",
  "Banco do Brasil",
  "Bradesco",
  "Caixa Econômica Federal",
  "Santander Brasil",
  "BTG Pactual",
  "Sicoob",
  "Sicredi",
  "Banco Safra",
  "Citibank Brasil"
];

const defaultItauForm = {
  sandboxClientId: "",
  sandboxClientSecret: "",
  productionClientId: "",
  productionClientSecret: "",
  certificatePassword: "",
  certificateFileName: "",
  certificateBase64: "",
  productionEnabled: false,
  serviceUrl: "",
  apiKey: "",
  accountIdentifier: ""
};

const webhookUrl = import.meta.env.VITE_ITAU_WEBHOOK_URL ?? "http://localhost:5089/integrations/itau/webhook";

export const BankConnectionsPage = () => {
  const [connections, setConnections] = useState<BankConnectionDto[]>([]);
  const toast = useToast();
  const [showApiModal, setShowApiModal] = useState(false);
  const [modalStep, setModalStep] = useState<1 | 2>(1);
  const [selectedBank, setSelectedBank] = useState(apiBanks[0]);
  const [itauForm, setItauForm] = useState(defaultItauForm);
  const [savingItau, setSavingItau] = useState(false);
  const [itauStatus, setItauStatus] = useState<BankIntegrationStatusDto>();
  const [testingIntegrationId, setTestingIntegrationId] = useState<string | null>(null);

  const formatDateTime = (value?: string) => (value ? new Date(value).toLocaleString("pt-BR") : "—");

  const getConnectionStatusDisplay = (status: BankConnectionDto["status"]) => {
    switch (status) {
      case "Active":
        return { label: "Concluído", className: "bg-green-50 text-green-700" };
      case "Pending":
        return { label: "Incompleto", className: "bg-amber-50 text-amber-700" };
      case "Revoked":
        return { label: "Revogado", className: "bg-slate-100 text-slate-600" };
      case "Error":
        return { label: "Erro", className: "bg-red-50 text-red-700" };
      default:
        return { label: status, className: "bg-slate-100 text-slate-600" };
    }
  };

  const getManualStatusDisplay = (configured: boolean) =>
    configured
      ? { label: "Concluído", className: "bg-green-50 text-green-700" }
      : { label: "Incompleto", className: "bg-amber-50 text-amber-700" };

  const manualIntegrations =
    itauStatus && itauStatus.integrationId
      ? [
          {
            id: itauStatus.integrationId,
            bank: itauStatus.bank ?? "Itaú Unibanco",
            createdAt: itauStatus.createdAt ?? itauStatus.updatedAt,
            configured: itauStatus.configured,
            productionEnabled: itauStatus.productionEnabled ?? false,
            lastTestedAt: itauStatus.lastTestedAt,
            serviceUrl: itauStatus.serviceUrl,
            apiKey: itauStatus.apiKey,
            accountIdentifier: itauStatus.accountIdentifier,
            status: itauStatus
          }
        ]
      : [];

  const loadConnections = () => {
    bankApi.list().then((response) => setConnections(response.data.data ?? []));
  };

  useEffect(() => {
    loadConnections();
    fetchItauStatus();
  }, []);

  const connect = async () => {
    const { data } = await bankApi.connect();
    toast.push(`Redirecionar usuário para ${data.data}`, "info");
    loadConnections();
  };

  const fetchItauStatus = () => {
    bankApi.getItauIntegration().then((response) => setItauStatus(response.data.data));
  };

  const buildItauFormFromStatus = (status?: BankIntegrationStatusDto) => ({
    sandboxClientId: status?.sandboxClientId ?? "",
    sandboxClientSecret: status?.sandboxClientSecret ?? "",
    productionClientId: status?.productionClientId ?? "",
    productionClientSecret: status?.productionClientSecret ?? "",
    certificatePassword: status?.certificatePassword ?? "",
    certificateFileName: status?.certificateFileName ?? "",
    certificateBase64: "",
    productionEnabled: status?.productionEnabled ?? false,
    serviceUrl: status?.serviceUrl ?? "",
    apiKey: status?.apiKey ?? "",
    accountIdentifier: status?.accountIdentifier ?? ""
  });

  const handleCertificateUpload = (file?: File) => {
    if (!file) {
      setItauForm((prev) => ({
        ...prev,
        certificateBase64: "",
        certificateFileName: prev.certificateBase64 ? "" : prev.certificateFileName
      }));
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result;
      if (result && typeof result !== "string") return;
      const base64 = typeof result === "string" ? result.split(",").pop() ?? "" : "";
      setItauForm((prev) => ({
        ...prev,
        certificateBase64: base64,
        certificateFileName: file.name
      }));
    };
    reader.readAsDataURL(file);
  };

  const handleSaveItau = async () => {
    const mustUploadCertificate =
      itauForm.productionEnabled && !itauForm.certificateBase64 && !itauStatus?.hasCertificate;
    if (mustUploadCertificate) {
      toast.push("Selecione o arquivo .pfx", "error");
      return;
    }
    if (!itauForm.apiKey.trim()) {
      toast.push("Informe a API Key", "error");
      return;
    }
    if (!itauForm.accountIdentifier.trim()) {
      toast.push("Informe o ID da conta (id_conta)", "error");
      return;
    }

    setSavingItau(true);
    try {
      const payload = {
        ...itauForm,
        certificateFileName: itauForm.certificateFileName || itauStatus?.certificateFileName || ""
      };
      const { data } = await bankApi.saveItauIntegration(payload);
      if (data.data) {
        setItauStatus(data.data);
      }
      toast.push("Credenciais Itaú salvas com sucesso.", "success");
      setShowApiModal(false);
      setModalStep(1);
      setItauForm(defaultItauForm);
    } catch (error) {
      toast.push("Não foi possível salvar as credenciais do Itaú.", "error");
    } finally {
      setSavingItau(false);
    }
  };

  const handleTestIntegration = async (integration: BankIntegrationStatusDto) => {
    if (!integration.integrationId) {
      return;
    }

    setTestingIntegrationId(integration.integrationId);
    try {
      const { data } = await bankApi.testItauIntegration({
        useProduction: integration.productionEnabled
      });
      if (data.data) {
        setItauStatus(data.data);
        toast.push("Teste concluído com sucesso.", "success");
      }
    } catch (error) {
      toast.push("Não foi possível testar a conexão.", "error");
    } finally {
      setTestingIntegrationId(null);
    }
  };

  const handleEditIntegration = (integration: BankIntegrationStatusDto) => {
    setSelectedBank("Itaú Unibanco");
    setItauForm(buildItauFormFromStatus(integration));
    setModalStep(2);
    setShowApiModal(true);
  };

  const copyWebhookUrl = async () => {
    await navigator.clipboard.writeText(webhookUrl);
    toast.push("URL copiada!", "success");
  };

  return (
    <>
      <div className="space-y-4">
        <div className="flex items-center justify-between gap-3 flex-wrap">
          <h1 className="text-2xl font-semibold">Conexões bancárias</h1>
          <div className="flex gap-2">
            <button className="btn-primary" onClick={connect}>
              Conectar via Open Finance
            </button>
            <button
              className="border border-slate-200 rounded px-4 py-2 text-sm font-semibold text-primary-600"
              onClick={() => {
                setModalStep(1);
                setSelectedBank(apiBanks[0]);
                setItauForm(defaultItauForm);
                fetchItauStatus();
                setShowApiModal(true);
              }}
            >
              Conectar via API/Webhook
            </button>
          </div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-slate-500 uppercase text-xs">
                <th className="px-4 py-2">Banco</th>
                <th>Status</th>
                <th>Data do cadastro</th>
                <th>Consentimento</th>
              </tr>
            </thead>
            <tbody>
              {connections.length === 0 && (
                <tr>
                  <td className="px-4 py-6 text-center text-slate-500" colSpan={4}>
                    Nenhuma conexão via Open Finance ainda.
                  </td>
                </tr>
              )}
              {connections.map((conn) => {
                const statusMeta = getConnectionStatusDisplay(conn.status);
                return (
                  <tr key={conn.id} className="border-t border-slate-100">
                    <td className="px-4 py-2">{conn.provider}</td>
                    <td>
                      <span
                        className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${statusMeta.className}`}
                      >
                        {statusMeta.label}
                      </span>
                    </td>
                    <td>{formatDateTime(conn.connectedAt)}</td>
                    <td className="font-mono text-xs text-slate-500">{conn.consentId}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white">
          <div className="px-4 py-3 border-b border-slate-100">
            <h2 className="font-semibold">Integrações via API/Webhook</h2>
            <p className="text-sm text-slate-500">Veja as conexões criadas manualmente com certificados e webhooks.</p>
          </div>
          {manualIntegrations.length === 0 ? (
            <div className="p-4 text-sm text-slate-500">Nenhuma integração via API/Webhook criada ainda.</div>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-slate-500 uppercase text-xs">
                  <th className="px-4 py-2">Banco</th>
                  <th>Ambiente</th>
                  <th>Status</th>
                  <th>Último teste</th>
                  <th>URL do serviço</th>
                  <th>ID da conta</th>
                  <th>Data do cadastro</th>
                  <th className="text-right">Ações</th>
                </tr>
              </thead>
              <tbody>
                {manualIntegrations.map((integration) => {
                  const statusMeta = getManualStatusDisplay(integration.configured);
                  return (
                    <tr key={integration.id} className="border-t border-slate-100">
                      <td className="px-4 py-2">{integration.bank}</td>
                      <td>{integration.productionEnabled ? "Produção" : "Sandbox"}</td>
                      <td>
                        <span
                          className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${statusMeta.className}`}
                        >
                          {statusMeta.label}
                        </span>
                      </td>
                      <td>{formatDateTime(integration.lastTestedAt)}</td>
                      <td className="font-mono text-xs text-slate-500 break-all">{integration.serviceUrl ?? "—"}</td>
                      <td className="font-mono text-xs text-slate-500 break-all">
                        {integration.accountIdentifier ?? "—"}
                      </td>
                      <td>{formatDateTime(integration.createdAt)}</td>
                      <td className="text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            className="text-sm font-semibold text-slate-600 border border-slate-200 rounded px-2 py-1 disabled:opacity-50"
                            disabled={testingIntegrationId === integration.id}
                            onClick={() => handleTestIntegration(integration.status)}
                          >
                            {testingIntegrationId === integration.id ? "Testando..." : "Testar"}
                          </button>
                          <button
                            className="text-sm font-semibold text-primary-600 hover:underline"
                            onClick={() => handleEditIntegration(integration.status)}
                          >
                            Editar
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>
      {showApiModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">
                {modalStep === 1 ? "Selecionar banco" : "Integração Itaú"}
              </h2>
              <button
                onClick={() => {
                  setShowApiModal(false);
                  setModalStep(1);
                  setItauForm(defaultItauForm);
                }}
                className="text-slate-500"
              >
                ✕
              </button>
            </div>
            {modalStep === 1 ? (
              <>
                <p className="text-sm text-slate-500 mb-4">
                  Escolha o banco para iniciar uma integração via API/Webhook.
                </p>
                <div className="max-h-64 overflow-y-auto space-y-2">
                  {apiBanks.map((bank) => (
                    <label
                      key={bank}
                      className="flex items-center gap-3 border border-slate-200 rounded-lg px-3 py-2 cursor-pointer hover:border-primary-400"
                    >
                      <input
                        type="radio"
                        name="bank"
                        value={bank}
                        checked={selectedBank === bank}
                        onChange={() => setSelectedBank(bank)}
                      />
                      <span className="text-sm">{bank}</span>
                      {bank === "Itaú Unibanco" && itauStatus?.integrationId && (
                        <span
                          className={`text-xs ml-auto ${itauStatus.configured ? "text-green-600" : "text-amber-600"}`}
                        >
                          {itauStatus.configured ? "Concluído" : "Incompleto"}
                        </span>
                      )}
                    </label>
                  ))}
                </div>
                <div className="mt-4 flex justify-end gap-2 text-sm">
                  <button
                    className="border border-slate-200 rounded px-3 py-2"
                    onClick={() => {
                      setShowApiModal(false);
                      setModalStep(1);
                    }}
                  >
                    Cancelar
                  </button>
                  <button
                    className="btn-primary"
                    onClick={() => {
                      if (selectedBank === "Itaú Unibanco") {
                        setItauForm(buildItauFormFromStatus(itauStatus));
                        setModalStep(2);
                        return;
                      }
                      toast.push(`Integração via API (${selectedBank}) em breve.`, "info");
                      setShowApiModal(false);
                    }}
                  >
                    {selectedBank === "Itaú Unibanco" ? "Continuar" : "Confirmar"}
                  </button>
                </div>
              </>
            ) : (
              <>
                <p className="text-sm text-slate-500 mb-2">
                  Preencha os dados obtidos no portal Itaú Developers (sandbox e produção) e envie o certificado .pfx.
                </p>
                {itauStatus?.integrationId && (
                  <div
                    className={`text-xs mb-2 ${itauStatus.configured ? "text-green-600" : "text-amber-600"}`}
                  >
                    {itauStatus.configured
                      ? `Último teste concluído em ${formatDateTime(itauStatus.lastTestedAt ?? itauStatus.updatedAt)}`
                      : "Integração pendente — finalize os campos e reenvie o certificado para concluir."}
                  </div>
                )}
                <label className="text-sm flex flex-col gap-1">
                  URL do serviço (opcional)
                  <input
                    className="border border-slate-200 rounded px-3 py-2"
                    placeholder="Deixe vazio para usar o endpoint oficial do Itaú"
                    value={itauForm.serviceUrl}
                    onChange={(e) => setItauForm((prev) => ({ ...prev, serviceUrl: e.target.value }))}
                  />
                  <span className="text-xs text-slate-500">Usamos automaticamente a URL padrão do Itaú Pix Recebimentos.</span>
                </label>
                <label className="text-sm flex flex-col gap-1">
                  API Key (x-itau-apikey)
                  <input
                    className="border border-slate-200 rounded px-3 py-2"
                    placeholder="GUID fornecido pelo Itaú Developers"
                    value={itauForm.apiKey}
                    onChange={(e) => setItauForm((prev) => ({ ...prev, apiKey: e.target.value }))}
                  />
                </label>
                <label className="text-sm flex flex-col gap-1">
                  ID da conta (id_conta)
                  <input
                    className="border border-slate-200 rounded px-3 py-2"
                    placeholder="ISPB+Agência(4)+Conta(13 sem DAC)"
                    value={itauForm.accountIdentifier}
                    onChange={(e) => setItauForm((prev) => ({ ...prev, accountIdentifier: e.target.value }))}
                  />
                  <span className="text-xs text-slate-500">
                    Ex.: ISPB 60701190, agência 1234, conta 0000000123456 → 6070119012340000000123456
                  </span>
                </label>
                <label className="flex items-start gap-3 border border-slate-200 rounded-lg px-3 py-2 text-sm">
                  <input
                    type="checkbox"
                    className="mt-1"
                    checked={itauForm.productionEnabled}
                    onChange={(e) =>
                      setItauForm((prev) => ({
                        ...prev,
                        productionEnabled: e.target.checked
                      }))
                    }
                  />
                  <div>
                    <span className="font-medium text-slate-700">Habilitar produção</span>
                    <p className="text-xs text-slate-500">
                      Quando ativo, os campos de sandbox ficam ocultos e o teste utilizará credenciais de produção.
                    </p>
                  </div>
                </label>
                <div className="space-y-3">
                  {!itauForm.productionEnabled && (
                    <>
                      <label className="text-sm flex flex-col gap-1">
                        Sandbox Client ID
                        <input
                          className="border border-slate-200 rounded px-3 py-2"
                          value={itauForm.sandboxClientId}
                          onChange={(e) => setItauForm((prev) => ({ ...prev, sandboxClientId: e.target.value }))}
                        />
                      </label>
                      <label className="text-sm flex flex-col gap-1">
                        Sandbox Client Secret
                        <input
                          className="border border-slate-200 rounded px-3 py-2"
                          value={itauForm.sandboxClientSecret}
                          onChange={(e) => setItauForm((prev) => ({ ...prev, sandboxClientSecret: e.target.value }))}
                        />
                      </label>
                    </>
                  )}
                  {itauForm.productionEnabled && (
                    <>
                      <label className="text-sm flex flex-col gap-1">
                        Produção Client ID
                        <input
                          className="border border-slate-200 rounded px-3 py-2"
                          value={itauForm.productionClientId}
                          onChange={(e) => setItauForm((prev) => ({ ...prev, productionClientId: e.target.value }))}
                        />
                      </label>
                      <label className="text-sm flex flex-col gap-1">
                        Produção Client Secret
                        <input
                          className="border border-slate-200 rounded px-3 py-2"
                          value={itauForm.productionClientSecret}
                          onChange={(e) =>
                            setItauForm((prev) => ({ ...prev, productionClientSecret: e.target.value }))
                          }
                        />
                      </label>
                      <label className="text-sm flex flex-col gap-1">
                        Senha do certificado (.pfx)
                        <input
                          type="password"
                          className="border border-slate-200 rounded px-3 py-2"
                          value={itauForm.certificatePassword}
                          onChange={(e) => setItauForm((prev) => ({ ...prev, certificatePassword: e.target.value }))}
                        />
                      </label>
                      <label className="text-sm flex flex-col gap-1">
                        Upload do certificado (.pfx)
                        <input
                          type="file"
                          accept=".pfx"
                          className="border border-dashed border-slate-300 rounded px-3 py-2"
                          onChange={(e) => handleCertificateUpload(e.target.files?.[0])}
                        />
                        {itauForm.certificateFileName && (
                          <span className="text-xs text-slate-500">
                            {itauForm.certificateBase64
                              ? `Arquivo selecionado: ${itauForm.certificateFileName}`
                              : `Arquivo atual: ${itauForm.certificateFileName}`}
                          </span>
                        )}
                        {!itauForm.certificateBase64 && itauStatus?.hasCertificate && (
                          <span className="text-xs text-slate-400">
                            Envie um novo certificado apenas se desejar substituir o atual.
                          </span>
                        )}
                      </label>
                    </>
                  )}
                  <div className="text-sm flex flex-col gap-2 bg-slate-50 border border-slate-200 rounded p-3">
                    <span>URL do Webhook (para colar no portal Itaú):</span>
                    <div className="flex items-center gap-2">
                      <code className="bg-white border border-slate-200 rounded px-2 py-1 text-xs flex-1 break-all">
                        {webhookUrl}
                      </code>
                      <button
                        type="button"
                        className="text-xs border border-slate-200 rounded px-2 py-1"
                        onClick={copyWebhookUrl}
                      >
                        Copiar
                      </button>
                    </div>
                  </div>
                </div>
                <div className="mt-4 flex justify-between gap-2 text-sm">
                  <button
                    className="border border-slate-200 rounded px-3 py-2"
                    onClick={() => setModalStep(1)}
                  >
                    Voltar
                  </button>
                  <button
                    className="btn-primary"
                    disabled={savingItau}
                    onClick={handleSaveItau}
                  >
                    {savingItau ? "Salvando..." : "Salvar"}
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </>
  );
};
