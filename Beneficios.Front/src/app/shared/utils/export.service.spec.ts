import { TestBed } from '@angular/core/testing';
import { ExportService } from './export.service';
import { ExportColumn } from './export.models';

describe('ExportService', () => {
  let service: ExportService;

  const rows: Record<string, unknown>[] = [
    { nome: 'João', email: 'joao@example.com', dataInclusao: '2024-01-15T10:30:00' },
    { nome: 'Maria', email: 'maria@example.com', dataInclusao: '2024-02-20T11:00:00' },
  ];

  const columns: ExportColumn[] = [
    { key: 'nome', label: 'Nome' },
    { key: 'email', label: 'Email' },
    {
      key: 'dataInclusao',
      label: 'Data inclusão',
      format: (value) => new Date(String(value)).toLocaleDateString('pt-BR'),
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExportService);
  });

  describe('buildSheetData', () => {
    it('deve usar labels das colunas como cabeçalhos', () => {
      const sheetData = service.buildSheetData(rows, columns);

      expect(Object.keys(sheetData[0])).toEqual(['Nome', 'Email', 'Data inclusão']);
    });

    it('deve manter a quantidade de linhas dos dados', () => {
      const sheetData = service.buildSheetData(rows, columns);

      expect(sheetData.length).toBe(2);
      expect(sheetData[0]['Nome']).toBe('João');
      expect(sheetData[1]['Email']).toBe('maria@example.com');
    });
  });

  describe('buildPdfTableData', () => {
    it('deve gerar cabeçalhos e linhas com contagem correta', () => {
      const tableData = service.buildPdfTableData(rows, columns);

      expect(tableData.head).toEqual([['Nome', 'Email', 'Data inclusão']]);
      expect(tableData.body.length).toBe(2);
      expect(tableData.body[0][0]).toBe('João');
    });
  });

  describe('exportToExcel', () => {
    it('deve gerar planilha sem erro com dados válidos', async () => {
      await expectAsync(
        service.exportToExcel(rows, columns, 'usuarios-admin-2026-07-06.xlsx'),
      ).toBeResolved();
    });
  });

  describe('buildFilename', () => {
    it('deve seguir padrão entidade-subdominio-data', () => {
      const filename = service.buildFilename('usuarios', 'empresa1', 'xlsx', new Date('2026-07-06T12:00:00'));

      expect(filename).toBe('usuarios-empresa1-2026-07-06.xlsx');
    });
  });
});
