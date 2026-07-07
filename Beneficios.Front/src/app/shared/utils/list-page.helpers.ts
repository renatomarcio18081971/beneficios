import { MatTable, MatTableDataSource } from '@angular/material/table';

export function applyTableData<T>(dataSource: MatTableDataSource<T>, data: T[]): void {
  dataSource.data = data;
  dataSource.paginator?.firstPage();
}

export function syncTableRows<T>(table?: MatTable<T>): void {
  if (table) {
    table.renderRows();
  }
}
