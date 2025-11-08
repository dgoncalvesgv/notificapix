import { useEffect, useState } from "react";
import { bankApi } from "../../../lib/api/client";
import type { BankConnectionDto } from "../../../lib/api/types";
import { useToast } from "../../../context/ToastContext";

export const BankConnectionsPage = () => {
  const [connections, setConnections] = useState<BankConnectionDto[]>([]);
  const toast = useToast();

  const load = () => {
    bankApi.list().then((response) => setConnections(response.data.data ?? []));
  };

  useEffect(() => {
    load();
  }, []);

  const connect = async () => {
    const { data } = await bankApi.connect();
    toast.push(`Redirecionar usuário para ${data.data}`, "info");
    load();
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Conexões bancárias</h1>
        <button className="btn-primary" onClick={connect}>
          Conectar via Open Finance
        </button>
      </div>
      <div className="rounded-xl border border-slate-200 bg-white">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-slate-500 uppercase text-xs">
              <th className="px-4 py-2">Provider</th>
              <th>Consent</th>
              <th>Status</th>
              <th>Conectado</th>
            </tr>
          </thead>
          <tbody>
            {connections.map((conn) => (
              <tr key={conn.id} className="border-t border-slate-100">
                <td className="px-4 py-2">{conn.provider}</td>
                <td>{conn.consentId}</td>
                <td>{conn.status}</td>
                <td>{conn.connectedAt ? new Date(conn.connectedAt).toLocaleString() : "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
