import { Injectable } from '@angular/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import { ExportColumn } from './export.models';

@Injectable({ providedIn: 'root' })
export class ExportService {
  buildSheetData(rows: Record<string, unknown>[], columns: ExportColumn[]): Record<string, unknown>[] {
    return rows.map((row) =>
      columns.reduce<Record<string, unknown>>((acc, column) => {
        const value = row[column.key];
        acc[column.label] = column.format ? column.format(value) : (value ?? '');
        return acc;
      }, {}),
    );
  }

  buildPdfTableData(rows: Record<string, unknown>[], columns: ExportColumn[]): {
    head: string[][];
    body: string[][];
  } {
    return {
      head: [columns.map((column) => column.label)],
      body: rows.map((row) =>
        columns.map((column) => {
          const value = row[column.key];
          const formatted = column.format ? column.format(value) : value;
          return formatted == null ? '' : String(formatted);
        }),
      ),
    };
  }

  buildFilename(
    entity: string,
    subdomain: string,
    extension: 'xlsx' | 'pdf',
    date: Date = new Date(),
  ): string {
    const datePart = date.toISOString().slice(0, 10);
    return `${entity}-${subdomain}-${datePart}.${extension}`;
  }

  exportToExcel<T extends object>(rows: T[], columns: ExportColumn[], filename: string): void {
    const worksheet = XLSX.utils.json_to_sheet(
      this.buildSheetData(rows as Record<string, unknown>[], columns),
    );
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Dados');
    XLSX.writeFile(workbook, filename);
  }

  exportToPdf<T extends object>(
    rows: T[],
    columns: ExportColumn[],
    filename: string,
    title: string,
  ): void {
    const doc = new jsPDF();
    const tableData = this.buildPdfTableData(rows as Record<string, unknown>[], columns);

    doc.setFontSize(14);
    doc.text(title, 14, 16);

    autoTable(doc, {
      head: tableData.head,
      body: tableData.body,
      startY: 22,
    });

    doc.save(filename);
  }
}
