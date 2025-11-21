const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

type MetricCardProps = {
  label: string;
  value: string | number;
  helper?: string;
  isCurrency?: boolean;
};

export const MetricCard = ({ label, value, helper, isCurrency }: MetricCardProps) => {
  const displayValue = isCurrency && typeof value === "number" ? currencyFormatter.format(value) : value;
  return (
    <div className="rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 p-4 shadow-sm">
      <p className="text-xs uppercase tracking-wide text-slate-500 dark:text-slate-300">{label}</p>
      <p className="text-2xl font-semibold mt-1 text-slate-900 dark:text-white">{displayValue}</p>
      {helper && <p className="text-xs text-slate-400 dark:text-slate-400 mt-1">{helper}</p>}
    </div>
  );
};
