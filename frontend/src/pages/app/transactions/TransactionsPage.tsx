import { useEffect, useState } from "react";
import { transactionsApi } from "../../../lib/api/client";
import type { PixTransactionDto } from "../../../lib/api/types";
import { DataTable } from "../../../components/DataTable";
import { useToast } from "../../../context/ToastContext";

const ExcelIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.5">
    <path d="M15 2H6a2 2 0 0 0-2 2v16c0 1.1.9 2 2 2h12a2 2 0 0 0 2-2V7z" />
    <path d="M15 2v5h5" />
    <path d="m9 10 6 6" />
    <path d="m15 10-6 6" />
  </svg>
);

const RefreshIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 12a9 9 0 1 1-3-6.7" />
    <path d="M21 3v6h-6" />
  </svg>
);

const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const toLocalISOString = (date: Date) => {
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60 * 1000);
  return local.toISOString();
};

export const TransactionsPage = () => {
  const now = new Date();
  const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
  const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  const defaultFrom = toLocalISOString(startOfMonth).split("T")[0];
  const defaultTo = toLocalISOString(endOfMonth).split("T")[0];

  const [items, setItems] = useState<PixTransactionDto[]>([]);
  const [fromDate, setFromDate] = useState(defaultFrom);
  const [toDate, setToDate] = useState(defaultTo);
  const [loading, setLoading] = useState(false);
  const [syncingBanks, setSyncingBanks] = useState(false);
  const [payerFilter, setPayerFilter] = useState("");
  const [descriptionFilter, setDescriptionFilter] = useState("");
  const toast = useToast();

  useEffect(() => {
    loadTransactions({ from: defaultFrom, to: defaultTo });
  }, [defaultFrom, defaultTo]);

  const loadTransactions = async (filters?: { from?: string; to?: string; payer?: string; description?: string }) => {
    setLoading(true);
    try {
      const payload: Record<string, unknown> = { page: 1, pageSize: 50 };
      if (filters?.from) payload.from = filters.from;
      if (filters?.to) payload.to = filters.to;
      if (filters?.payer) payload.payerName = filters.payer;
      if (filters?.description) payload.description = filters.description;
      const response = await transactionsApi.list(payload);
      setItems(response.data.data?.items ?? []);
    } finally {
      setLoading(false);
    }
  };

  const handleFilter = (event: React.FormEvent) => {
    event.preventDefault();
    const filters: { from?: string; to?: string; payer?: string; description?: string } = {};
    if (fromDate) {
      const start = new Date(`${fromDate}T00:00:00`);
      filters.from = toLocalISOString(start).split("T")[0];
    }
    if (toDate) {
      const end = new Date(`${toDate}T23:59:59`);
      filters.to = toLocalISOString(end).split("T")[0];
    }
    if (payerFilter.trim()) {
      filters.payer = payerFilter.trim();
    }
    if (descriptionFilter.trim()) {
      filters.description = descriptionFilter.trim();
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

  const averageAmount =
    items.length > 0 ? items.reduce((sum, item) => sum + item.amount, 0) / items.length : 0;
  const highestPayer = items.length
    ? items.reduce((prev, current) => (current.amount > prev.amount ? current : prev))
    : undefined;
  const payerAggregates = items.reduce<Record<string, { total: number; count: number; name: string }>>(
    (acc, item) => {
      if (!acc[item.payerName]) {
        acc[item.payerName] = { total: 0, count: 0, name: item.payerName };
      }
      acc[item.payerName].total += item.amount;
      acc[item.payerName].count += 1;
      return acc;
    },
    {}
  );
  const topPayerByTotal = Object.values(payerAggregates).sort((a, b) => b.total - a.total)[0];

  return (
    <div className="space-y-4">
      <header className="space-y-3">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <h1 className="text-2xl font-semibold">Transações PIX</h1>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={handleSyncBanks}
              className="border border-slate-200 rounded px-4 py-2 text-sm font-semibold text-primary-600 disabled:opacity-60 flex items-center gap-2"
              disabled={syncingBanks}
              title="Sincronizar integrações bancárias"
            >
              <RefreshIcon />
              {syncingBanks ? "Sincronizando..." : "Baixar integrações concluídas"}
            </button>
          </div>
        </div>
        <form
          onSubmit={handleFilter}
          className="grid gap-3 text-sm md:grid-cols-[150px_150px_minmax(0,1fr)_minmax(0,1fr)_max-content]"
        >
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">De</span>
            <input type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} className="input" />
          </label>
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">Até</span>
            <input type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} className="input" />
          </label>
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">Pagador</span>
            <input
              type="text"
              value={payerFilter}
              onChange={(event) => setPayerFilter(event.target.value)}
              className="input"
              placeholder="Nome do pagador"
            />
          </label>
          <label className="flex flex-col">
            <span className="text-xs text-slate-500 mb-1">Descrição</span>
            <input
              type="text"
              value={descriptionFilter}
              onChange={(event) => setDescriptionFilter(event.target.value)}
              className="input"
              placeholder="Descrição do Pix"
            />
          </label>
          <div className="flex gap-2 items-end justify-end">
            <button type="submit" className="btn-primary whitespace-nowrap" disabled={loading}>
              {loading ? "Filtrando..." : "Filtrar"}
            </button>
            <button
              type="button"
              onClick={handleExport}
              className="inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm font-semibold text-primary-600 disabled:opacity-50"
              disabled={!items.length}
              title="Exportar Excel"
            >
              <ExcelIcon />
              <span>Exportar Excel</span>
            </button>
          </div>
        </form>
      </header>
      <section className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="rounded-xl border border-slate-200 bg-white p-4 dark:bg-slate-800/80 dark:border-slate-600">
          <p className="text-xs uppercase text-slate-500 dark:text-slate-200">Média de PIX</p>
          <p className="text-2xl font-semibold text-slate-800 dark:text-white">
            {items.length ? currencyFormatter.format(averageAmount) : "—"}
          </p>
          <p className="text-xs text-slate-500 dark:text-slate-200 mt-1">
            Baseado nas transações carregadas ({items.length})
          </p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 dark:bg-slate-800/80 dark:border-slate-600">
          <p className="text-xs uppercase text-slate-500 dark:text-slate-200">Maior pagador</p>
          <p className="text-lg font-semibold text-slate-800 dark:text-white">{highestPayer?.payerName ?? "—"}</p>
          <p className="text-sm text-slate-500 dark:text-slate-200">
            {highestPayer ? currencyFormatter.format(highestPayer.amount) : "Sem registros"}
          </p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 dark:bg-slate-800/80 dark:border-slate-600">
          <p className="text-xs uppercase text-slate-500 dark:text-slate-200">Cliente com maior volume</p>
          <p className="text-lg font-semibold text-slate-800 dark:text-white">{topPayerByTotal?.name ?? "—"}</p>
          <p className="text-sm text-slate-500 dark:text-slate-200">
            {topPayerByTotal
              ? `${currencyFormatter.format(topPayerByTotal.total)} • ${topPayerByTotal.count} transações`
              : "Sem registros"}
          </p>
        </div>
      </section>
      <DataTable
        data={items}
        columns={[
          { header: "Valor", accessor: (row) => currencyFormatter.format(row.amount), sortKey: "amount" },
          { header: "Pagador", accessor: (row) => row.payerName, sortKey: "payerName" },
          { header: "Descrição", accessor: (row) => row.description },
          { header: "Data", accessor: (row) => new Date(row.occurredAt).toLocaleString("pt-BR"), sortKey: "occurredAt" }
        ]}
        emptyMessage={loading ? "Carregando..." : "Nenhuma transação encontrada"}
      />
    </div>
  );
};
