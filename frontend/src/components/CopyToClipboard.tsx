import { useState } from "react";

type Props = {
  value: string;
};

export const CopyToClipboard = ({ value }: Props) => {
  const [copied, setCopied] = useState(false);
  return (
    <button
      onClick={async () => {
        await navigator.clipboard.writeText(value);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
      }}
      className="text-xs uppercase tracking-wide px-2 py-1 border border-slate-200 rounded"
    >
      {copied ? "Copiado" : "Copiar"}
    </button>
  );
};
