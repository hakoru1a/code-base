export enum WeighTicketStatus {
  All = 'ALL',
  Active = 'ACTIVE', // xe còn trong xưởng: đã cân vào, chưa cân ra
  Completed = 'COMPLETED', // đã cân ra
  Cancelled = 'CANCELLED'
}

export enum WeighTicketType {
  In = 'IN',
  Out = 'OUT'
}

export enum PaymentStatus {
  Paid = 'PAID',
  Unpaid = 'UNPAID'
}

export enum WeighTicketMode {
  View = 'VIEW',
  Qc = 'QC'
}
