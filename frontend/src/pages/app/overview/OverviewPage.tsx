import { useEffect, useState } from "react";
import { MetricCard } from "../../../components/MetricCard";
import { DataTable } from "../../../components/DataTable";
import { dashboardApi } from "../../../lib/api/client";
import type { OverviewMetricsResponse } from "../../../lib/api/types";
import { Loader } from "../../../components/Loader";

export const OverviewPage = () => {
  const [data, setData] = useState<OverviewMetricsResponse>();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    dashboardApi
      .overview()
      .then((response) => setData(response.data.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Loader />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Overview</h1>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <MetricCard label="PIX hoje" value={`R$ ${data?.todayTotal?.toFixed(2) ?? "0,00"}`} />
        <MetricCard label="Últimos 7 dias" value={`R$ ${data?.last7DaysTotal?.toFixed(2) ?? "0,00"}`} />
        <MetricCard label="Últimos 30 dias" value={`R$ ${data?.last30DaysTotal?.toFixed(2) ?? "0,00"}`} />
      </div>
      <section>
        <h2 className="font-semibold mb-2">Últimos PIX</h2>
        <DataTable
          data={data?.recentTransactions ?? []}
          columns={[
            { header: "Valor", accessor: (row) => `R$ ${row.amount.toFixed(2)}` },
            { header: "Pagador", accessor: (row) => row.payerName },
            { header: "Data", accessor: (row) => new Date(row.occurredAt).toLocaleString() }
          ]}
        />
      </section>
    </div>
  );
};
