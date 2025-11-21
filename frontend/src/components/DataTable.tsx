import { useState, type ReactNode } from "react";

type Column<T> = {
  header: string;
  accessor: (row: T) => ReactNode;
  sortKey?: keyof T;
};

type DataTableProps<T> = {
  data: T[];
  columns: Column<T>[];
  emptyMessage?: string;
};

export const DataTable = <T,>({ data, columns, emptyMessage = "Sem dados" }: DataTableProps<T>) => {
  const [sortColumn, setSortColumn] = useState<string | null>(null);
  const [sortAsc, setSortAsc] = useState(true);

  if (!data.length) {
    return <p className="text-sm text-slate-500 dark:text-slate-300">{emptyMessage}</p>;
  }

  const sortedData = [...data].sort((a, b) => {
    if (!sortColumn) return 0;
    const column = columns.find((c) => c.header === sortColumn);
    if (!column?.sortKey) return 0;
    const aValue = (a as Record<string, unknown>)[column.sortKey];
    const bValue = (b as Record<string, unknown>)[column.sortKey];
    if (aValue === bValue) return 0;
    if (aValue === undefined || aValue === null) return sortAsc ? -1 : 1;
    if (bValue === undefined || bValue === null) return sortAsc ? 1 : -1;
    if (typeof aValue === "number" && typeof bValue === "number") {
      return sortAsc ? aValue - bValue : bValue - aValue;
    }
    return sortAsc
      ? String(aValue).localeCompare(String(bValue))
      : String(bValue).localeCompare(String(aValue));
  });

  const handleSort = (column: Column<T>) => {
    if (sortColumn === column.header) {
      setSortAsc((prev) => !prev);
    } else {
      setSortColumn(column.header);
      setSortAsc(true);
    }
  };

  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900">
      <table className="min-w-full text-sm">
        <thead>
          <tr>
            {columns.map((column) => (
              <th
                key={column.header}
                className={`text-left font-semibold px-4 py-2 ${
                  column.sortKey
                    ? "text-primary-600 cursor-pointer select-none"
                    : "text-slate-500 dark:text-slate-300"
                }`}
                onClick={() => column.sortKey && handleSort(column)}
              >
                {column.header}
                {column.sortKey && sortColumn === column.header && (
                  <span className="ml-1 text-xs">{sortAsc ? "▲" : "▼"}</span>
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {sortedData.map((row, index) => (
            <tr
              key={index}
              className={`border-t border-slate-100 dark:border-slate-800 ${
                index % 2 === 0 ? "bg-white dark:bg-slate-800/70" : "bg-slate-50 dark:bg-slate-900/60"
              }`}
            >
              {columns.map((column) => (
                <td key={column.header} className="px-4 py-2">
                  {column.accessor(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
