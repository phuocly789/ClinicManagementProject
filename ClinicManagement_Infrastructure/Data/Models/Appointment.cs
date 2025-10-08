using System;
using System.Collections.Generic;

namespace ClinicManagement_Infrastructure.Infrastructure.Data.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? PatientId { get; set; }

    public int? StaffId { get; set; }

    public int? ScheduleId { get; set; }

    public int? RecordId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? FollowUpDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }
    public string? Note { get; set; }

    public virtual User1? CreatedByNavigation { get; set; }

    public virtual ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User1? Patient { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<Queue> Queues { get; set; } = new List<Queue>();

    public virtual MedicalRecord? Record { get; set; }

    public virtual StaffSchedule? Schedule { get; set; }

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual User1? Staff { get; set; }
}
