export interface ExportColumn {
  key: string;
  label: string;
  format?: (value: unknown) => string;
}
