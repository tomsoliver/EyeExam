using EyeExamApi.Core.DTOs;

namespace EyeExamApi.Interfaces
{
    public interface IParsedScheduleDataService
    {
        public IEnumerable<ParsedScheduleNoticeOfLease> GetParsedScheduleNoticeOfLeases();
    }
}