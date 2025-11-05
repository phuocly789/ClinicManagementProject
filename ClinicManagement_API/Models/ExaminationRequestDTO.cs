public class ExaminationRequestDto
{
    public int QueueId { get; set; }
    public string Symptoms { get; set; }
    public string Diagnosis { get; set; }
    public List<PrescriptionDetailDto> Prescriptions { get; set; } // Tái sử dụng DTO kê đơn
    public List<int> ServiceIds { get; set; } // Danh sách ID các dịch vụ cần chỉ định
    public bool IsComplete { get; set; } // Cờ để biết là tạm lưu hay hoàn tất
}

public class PrescriptionDetailDto
{
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public string DosageInstruction { get; set; }
}
