import { useEffect, useState } from "react";
import { transactionsApi } from "../../../lib/api/client";
import type { PixTransactionDto } from "../../../lib/api/types";
import { DataTable } from "../../../components/DataTable";
import { useToast } from "../../../context/ToastContext";

const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const toLocalISOString = (date: Date) => {
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60 * 1000);
  return local.toISOString();
};

export const TransactionsPage = () => {
  const [items, setItems] = useState<PixTransactionDto[]>([]);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [loading, setLoading] = useState(false);
  const [syncingBanks, setSyncingBanks] = useState(false);
  const toast = useToast();

  useEffect(() => {
    loadTransactions();
  }, []);

  const loadTransactions = async (filters?: { from?: string; to?: string }) => {
    setLoading(true);
    try {
      const payload: Record<string, unknown> = { page: 1, pageSize: 50 };
      if (filters?.from) payload.from = filters.from;
      if (filters?.to) payload.to = filters.to;
      const response = await transactionsApi.list(payload);
      setItems(response.data.data?.items ?? []);
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = (event: React.FormEvent) => {
    event.preventDefault();
    const filters: { from?: string; to?: string } = {};
    if (fromDate) {
      const start = new Date(`${fromDate}T00:00:00`);
      filters.from = toLocalISOString(start).split("T")[0];
    }
    if (toDate) {
      const end = new Date(`${toDate}T23:59:59`);
      filters.to = toLocalISOString(end).split("T")[0];
    }
    loadTransactions(filters);
  };

  const handleExport = () => {
    if (!items.length) return;
    const header = ["Data", "Valor", "Pagador", "Descrição"];
    const rows = items.map((item) => [
      new Date(item.occurredAt).toLocaleString("pt-BR"),
      item.amount.toFixed(2).replace(".", ","),
      item.payerName,
      item.description?.replace(/;/g, ",")
    ]);
    const csvContent =
      [header, ...rows]
        .map((row) => row.map((value) => `"${value ?? ""}"`).join(";"))
        .join("\n");
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    const filename = `transacoes_${fromDate || "inicio"}_${toDate || "fim"}.csv`;
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const handleSyncBanks = async () => {
    setSyncingBanks(true);
    try {
      const { data } = await transactionsApi.syncBanks();
      const message = data.data?.message ?? "Sincronização iniciada.";
      toast.push(message, "success");
      await loadTransactions();
    } catch (error) {
      toast.push("Não foi possível sincronizar agora.", "error");
    } finally {
      setSyncingBanks(false);
    }
  };

  return (
    <div className="space-y-4">
      <header className="space-y-3">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <h1 className="text-2xl font-semibold">Transações PIX</h1>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={handleSyncBanks}
              className="border border-slate-200 rounded px-4 py-2 text-sm font-semibold text-primary-600 disabled:opacity-60"
              disabled={syncingBanks}
            >
              {syncingBanks ? "Sincronizando..." : "Baixar integrações concluídas"}
            </button>
          </div>
        </div>
        <form onSubmit={handleFilter} className="flex flex-wrap items-end gap-3 text-sm">
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">De</span>
            <input
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
              className="border border-slate-200 rounded px-3 py-2"
            />
          </label>
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">Até</span>
            <input
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
              className="border border-slate-200 rounded px-3 py-2"
            />
          </label>
          <div className="flex gap-2">
            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? "Filtrando..." : "Filtrar"}
            </button>
            <button
              type="button"
              onClick={handleExport}
              className="border border-slate-200 rounded px-3 py-2 text-sm font-semibold text-primary-600"
              disabled={!items.length}
            >
              Exportar Excel
            </button>
          </div>
        </form>
      </header>
      <DataTable
        data={items}
        columns={[
          { header: "Valor", accessor: (row) => currencyFormatter.format(row.amount) },
          { header: "Pagador", accessor: (row) => row.payerName },
          { header: "Descrição", accessor: (row) => row.description },
          { header: "Data", accessor: (row) => new Date(row.occurredAt).toLocaleString("pt-BR") }
        ]}
        emptyMessage={loading ? "Carregando..." : "Nenhuma transação encontrada"}
      />
    </div>
  );
};
