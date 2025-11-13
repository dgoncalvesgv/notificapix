import { useEffect, useState } from "react";
import { MetricCard } from "../../../components/MetricCard";
import { DataTable } from "../../../components/DataTable";
import { dashboardApi, transactionsApi } from "../../../lib/api/client";
import type { OverviewMetricsResponse, PixTransactionDto } from "../../../lib/api/types";
import { Loader } from "../../../components/Loader";
import { ResponsiveContainer, LineChart, Line, CartesianGrid, XAxis, YAxis, Tooltip, BarChart, Bar } from "recharts";
const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

export const OverviewPage = () => {
  const [data, setData] = useState<OverviewMetricsResponse>();
  const [loading, setLoading] = useState(true);
  const [todaySeries, setTodaySeries] = useState<Array<{ hour: string; total: number }>>([]);
  const [monthSeries, setMonthSeries] = useState<Array<{ day: string; total: number }>>([]);

  useEffect(() => {
    dashboardApi
      .overview()
      .then((response) => setData(response.data.data))
      .finally(() => setLoading(false));
    loadCharts();
  }, []);

  const loadCharts = async () => {
    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);

    const [todayRes, monthRes] = await Promise.all([
      transactionsApi.list({
        from: startOfToday.toISOString(),
        to: now.toISOString(),
        page: 1,
        pageSize: 500
      }),
      transactionsApi.list({
        from: startOfMonth.toISOString(),
        to: now.toISOString(),
        page: 1,
        pageSize: 500
      })
    ]);

    setTodaySeries(buildHourlySeries(todayRes.data.data?.items ?? [], startOfToday));
    setMonthSeries(buildDailySeries(monthRes.data.data?.items ?? [], startOfMonth, now));
  };

  const buildHourlySeries = (transactions: PixTransactionDto[], dayStart: Date) => {
    const hours = Array.from({ length: 24 }, (_, idx) => ({
      hour: `${idx.toString().padStart(2, "0")}:00`,
      total: 0
    }));
    transactions.forEach((tx) => {
      const occurred = new Date(tx.occurredAt);
      const diffHours = occurred.getHours();
      hours[diffHours].total += tx.amount;
    });
    return hours;
  };

  const buildDailySeries = (transactions: PixTransactionDto[], monthStart: Date, now: Date) => {
    const daysInMonth = new Date(monthStart.getFullYear(), monthStart.getMonth() + 1, 0).getDate();
    const days = Array.from({ length: daysInMonth }, (_, idx) => ({
      day: (idx + 1).toString().padStart(2, "0"),
      total: 0
    }));
    transactions.forEach((tx) => {
      const occurred = new Date(tx.occurredAt);
      if (occurred >= monthStart && occurred <= now) {
        const dayIndex = occurred.getDate() - 1;
        days[dayIndex].total += tx.amount;
      }
    });
    return days;
  };

  if (loading) return <Loader />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Overview</h1>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <MetricCard label="PIX hoje" value={data?.todayTotal ?? 0} isCurrency />
        <MetricCard label="Últimos 7 dias" value={data?.last7DaysTotal ?? 0} isCurrency />
        <MetricCard label="Últimos 30 dias" value={data?.last30DaysTotal ?? 0} isCurrency />
      </div>
      <section>
        <h2 className="font-semibold mb-2">Últimos PIX</h2>
        <DataTable
          data={data?.recentTransactions ?? []}
          columns={[
            { header: "Valor", accessor: (row) => currencyFormatter.format(row.amount) },
            { header: "Pagador", accessor: (row) => row.payerName },
            { header: "Data", accessor: (row) => new Date(row.occurredAt).toLocaleString() }
          ]}
        />
      </section>
      <section className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4">
          <h3 className="font-semibold mb-2">Transações do dia (por hora)</h3>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={todaySeries}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="hour" tick={{ fontSize: 12 }} />
                <YAxis tickFormatter={(value) => currencyFormatter.format(value)} />
                <Tooltip formatter={(value: number) => currencyFormatter.format(value)} />
                <Line type="monotone" dataKey="total" stroke="#2563eb" strokeWidth={2} dot={false} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4">
          <h3 className="font-semibold mb-2">Transações do mês (por dia)</h3>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={monthSeries}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="day" tick={{ fontSize: 12 }} />
                <YAxis tickFormatter={(value) => currencyFormatter.format(value)} />
                <Tooltip formatter={(value: number) => currencyFormatter.format(value)} />
                <Bar dataKey="total" fill="#10b981" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </section>
    </div>
  );
};
