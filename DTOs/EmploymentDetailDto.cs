namespace HRSystem.API.DTOs
{
    public class EmploymentDetailDto
    {
        public int EmploymentDetailId { get; set; }
        public int EmployeeId { get; set; }

        public DateTime JoiningDate { get; set; }
        public string Category { get; set; }
        public string OfferDesignation { get; set; }
        public string MOLDesignation { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal AccommodationAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal MOLBasicSalary { get; set; }
        public decimal MOLGrossSalary { get; set; }
        public decimal CurrentGrossSalary { get; set; }

        public bool OT_Eligible { get; set; }
        public string SalaryMode { get; set; }
        public string BankDetails { get; set; }
        public string BankAccountNo { get; set; }
        public string IBAN { get; set; }

        public string WorkLocation { get; set; }
        public string ContractType { get; set; }
        public string OtherSalaryDetails { get; set; }

        public string VisaNo { get; set; }
        public DateTime VisaIssueDate { get; set; }
        public DateTime VisaExpiry { get; set; }

        public string EmiratesId { get; set; }
        public DateTime EmiratesIssueDate { get; set; }
        public DateTime EmiratesExpiry { get; set; }

        public string LaborCardNo { get; set; }
        public DateTime LaborCardIssueDate { get; set; }
        public DateTime LaborCardExpiry { get; set; }

        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }




    public class CreateEmploymentDetailDto
    {
        public int EmployeeId { get; set; }

        public DateTime JoiningDate { get; set; }
        public string Category { get; set; }
        public string OfferDesignation { get; set; }
        public string MOLDesignation { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal AccommodationAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal MOLBasicSalary { get; set; }
        public decimal MOLGrossSalary { get; set; }
        public decimal CurrentGrossSalary { get; set; }

        public bool OT_Eligible { get; set; }
        public string SalaryMode { get; set; }
        public string BankDetails { get; set; }
        public string BankAccountNo { get; set; }
        public string IBAN { get; set; }

        public string WorkLocation { get; set; }
        public string ContractType { get; set; }
        public string OtherSalaryDetails { get; set; }

        public string VisaNo { get; set; }
        public DateTime VisaIssueDate { get; set; }
        public DateTime VisaExpiry { get; set; }

        public string EmiratesId { get; set; }
        public DateTime EmiratesIssueDate { get; set; }
        public DateTime EmiratesExpiry { get; set; }

        public string LaborCardNo { get; set; }
        public DateTime LaborCardIssueDate { get; set; }
        public DateTime LaborCardExpiry { get; set; }

        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateEmploymentDetailDto
    {
        public int EmployeeId { get; set; }

        public DateTime JoiningDate { get; set; }
        public string Category { get; set; }
        public string OfferDesignation { get; set; }
        public string MOLDesignation { get; set; }

        public decimal BasicSalary { get; set; }
        public decimal AccommodationAllowance { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal MOLBasicSalary { get; set; }
        public decimal MOLGrossSalary { get; set; }
        public decimal CurrentGrossSalary { get; set; }

        public bool OT_Eligible { get; set; }
        public string SalaryMode { get; set; }
        public string BankDetails { get; set; }
        public string BankAccountNo { get; set; }
        public string IBAN { get; set; }

        public string WorkLocation { get; set; }
        public string ContractType { get; set; }
        public string OtherSalaryDetails { get; set; }

        public string VisaNo { get; set; }
        public DateTime VisaIssueDate { get; set; }
        public DateTime VisaExpiry { get; set; }

        public string EmiratesId { get; set; }
        public DateTime EmiratesIssueDate { get; set; }
        public DateTime EmiratesExpiry { get; set; }

        public string LaborCardNo { get; set; }
        public DateTime LaborCardIssueDate { get; set; }
        public DateTime LaborCardExpiry { get; set; }

        public string Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
