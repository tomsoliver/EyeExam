using EyeExamApi.Core.DTOs;

namespace EyeExamApi.Core.Interfaces
{
    public interface IParsedScheduleDataService
    {
        public IEnumerable<ParsedScheduleNoticeOfLease> GetParsedScheduleNoticeOfLeases();
    }
}