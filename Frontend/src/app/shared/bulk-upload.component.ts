import { Component, EventEmitter, Input, Output } from '@angular/core';
import * as XLSX from 'xlsx';
import { BulkResult } from '../core/models';
import { NotificationService } from '../core/services/ui.service';

export interface BulkColumn {
  key: string;
  label: string;
  example: string;
}

@Component({
  selector: 'app-bulk-upload',
  standalone: false,
  template: `
    <p class="text-muted mb-2">
      Seleccione un archivo <strong>.xlsx</strong> o <strong>.csv</strong> con estas columnas:
      <span class="badge text-bg-light border me-1" *ngFor="let column of columns">{{ column.label }}</span>
    </p>

    <div class="d-flex flex-wrap gap-2 align-items-center mb-3">
      <input type="file" class="form-control w-auto" accept=".xlsx,.csv" (change)="onFileSelected($event)" />
      <button type="button" class="btn btn-outline-secondary btn-sm" (click)="downloadTemplate()">
        <span class="material-icons">download</span> Descargar plantilla
      </button>
    </div>

    <div *ngIf="rows.length > 0 && !result">
      <div class="alert alert-info py-2">Se leyeron <strong>{{ rows.length }}</strong> filas del archivo.</div>
      <button type="button" class="btn btn-primary" (click)="upload.emit(rows)">
        <span class="material-icons">upload</span> Cargar {{ rows.length }} registros
      </button>
    </div>

    <div *ngIf="result">
      <div class="alert py-2" [ngClass]="result.failed === 0 ? 'alert-success' : 'alert-warning'">
        Total: <strong>{{ result.total }}</strong> · Creados: <strong>{{ result.created }}</strong> · Rechazados:
        <strong>{{ result.failed }}</strong>
      </div>
      <div class="table-responsive" style="max-height: 300px">
        <table class="table table-sm">
          <thead>
            <tr>
              <th>Fila</th>
              <th>Registro</th>
              <th>Resultado</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of result.rows">
              <td>{{ row.row }}</td>
              <td>{{ row.key }}</td>
              <td [ngClass]="row.success ? 'text-success' : 'text-danger'">{{ row.message }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
})
export class BulkUploadComponent {
  @Input() columns: BulkColumn[] = [];
  @Input() templateName = 'plantilla';
  @Input() result: BulkResult | null = null;
  @Output() upload = new EventEmitter<Record<string, string>[]>();

  rows: Record<string, string>[] = [];

  constructor(private notifications: NotificationService) {}

  async onFileSelected(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0];
    this.rows = [];
    this.result = null;

    if (!file) {
      return;
    }

    try {
      const buffer = await file.arrayBuffer();
      // raw: true hace que en un CSV no se conviertan a número valores como 0991234567 (perdería el cero)
      const workbook = XLSX.read(buffer, { type: 'array', raw: true });
      const sheet = workbook.Sheets[workbook.SheetNames[0]];
      const rawRows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, { defval: '', raw: false });

      this.rows = rawRows.map((rawRow) => this.mapRow(rawRow)).filter((row) => Object.values(row).some((value) => value !== ''));

      if (this.rows.length === 0) {
        this.notifications.warning('El archivo no tiene filas con datos');
      }
    } catch {
      this.notifications.error('No se pudo leer el archivo. Verifique que sea .xlsx o .csv');
    }
  }

  downloadTemplate(): void {
    const sheet = XLSX.utils.aoa_to_sheet([this.columns.map((column) => column.label), this.columns.map((column) => column.example)]);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, sheet, 'Datos');
    XLSX.writeFile(workbook, `${this.templateName}.xlsx`);
  }

  private mapRow(rawRow: Record<string, unknown>): Record<string, string> {
    const normalized = new Map<string, string>();
    for (const [header, value] of Object.entries(rawRow)) {
      normalized.set(this.normalize(header), String(value ?? '').trim());
    }

    const row: Record<string, string> = {};
    for (const column of this.columns) {
      row[column.key] = normalized.get(this.normalize(column.label)) ?? normalized.get(this.normalize(column.key)) ?? '';
    }
    return row;
  }

  private normalize(text: string): string {
    return text
      .toLowerCase()
      .normalize('NFD')
      .replace(/[̀-ͯ]/g, '')
      .replace(/[^a-z0-9]/g, '');
  }
}
