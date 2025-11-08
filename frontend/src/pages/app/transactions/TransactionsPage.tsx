import { useEffect, useState } from "react";
import { transactionsApi } from "../../../lib/api/client";
import type { PixTransactionDto } from "../../../lib/api/types";
import { DataTable } from "../../../components/DataTable";

export const TransactionsPage = () => {
  const [items, setItems] = useState<PixTransactionDto[]>([]);

  useEffect(() => {
    transactionsApi
      .list({ page: 1, pageSize: 20 })
      .then((response) => setItems(response.data.data?.items ?? []));
  }, []);

  return (
    <div className="space-y-4">
      <header className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Transações PIX</h1>
      </header>
      <DataTable
        data={items}
        columns={[
          { header: "Valor", accessor: (row) => `R$ ${row.amount.toFixed(2)}` },
          { header: "Pagador", accessor: (row) => row.payerName },
          { header: "Descrição", accessor: (row) => row.description },
          { header: "Data", accessor: (row) => new Date(row.occurredAt).toLocaleString() }
        ]}
      />
    </div>
  );
};
