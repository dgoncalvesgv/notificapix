import { useEffect, useState } from "react";
import { alertsApi } from "../../../lib/api/client";
import type { AlertDto } from "../../../lib/api/types";
import { DataTable } from "../../../components/DataTable";
import { useToast } from "../../../context/ToastContext";

export const AlertsPage = () => {
  const [alerts, setAlerts] = useState<AlertDto[]>([]);
  const toast = useToast();

  const load = () => {
    alertsApi.list({ page: 1, pageSize: 20 }).then((response) => setAlerts(response.data.data?.items ?? []));
  };

  useEffect(() => {
    load();
  }, []);

  const sendTest = async () => {
    await alertsApi.test({ amount: 123.45, payerName: "Teste", payerKey: "teste@pix", description: "Envio manual" });
    toast.push("Teste enviado", "success");
    load();
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Alertas</h1>
        <button className="btn-primary" onClick={sendTest}>
          Enviar teste
        </button>
      </div>
      <DataTable
        data={alerts}
        columns={[
          { header: "Canal", accessor: (row) => row.channel },
          { header: "Status", accessor: (row) => row.status },
          { header: "Tentativas", accessor: (row) => row.attempts },
          { header: "Última tentativa", accessor: (row) => row.lastAttemptAt ? new Date(row.lastAttemptAt).toLocaleString() : "—" }
        ]}
      />
    </div>
  );
};
