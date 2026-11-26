import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BulkUploadComponent } from './bulk-upload.component';
import { ModalComponent } from './modal.component';

@NgModule({
  declarations: [ModalComponent, BulkUploadComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  exports: [CommonModule, FormsModule, ReactiveFormsModule, ModalComponent, BulkUploadComponent],
})
export class SharedModule {}
