using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eminutes.UWP.Model
{
    public class Rootobject
    {
        public int id { get; set; }
        public string meetingTitle { get; set; }
        public string meetingDescription { get; set; }
        public string meetingLocation { get; set; }
        public DateTime meetingDate { get; set; }
        public string meetingStartTime { get; set; }
        public DateTime actualMeetingStartTime { get; set; }
        public string meetingEndTime { get; set; }
        public DateTime actualMeetingEndTime { get; set; }
        public int organizedById { get; set; }
        public Organizedby organizedBy { get; set; }
        public List<Meetingparticipant> meetingParticipants { get; set; }
        public List<Meetingagendaitem> meetingAgendaItems { get; set; }
        public object meetingOrders { get; set; }
        public object meetingMinutesDoc { get; set; }
        public string meetingRefId { get; set; }
        public List<object> meetingExternalParticipants { get; set; }
        public object addMinutesStatus { get; set; }
        public string meetingStatus { get; set; }
        public object locationLongitude { get; set; }
        public object locationLatitude { get; set; }
        public object meetingURL { get; set; }
        public string meetingType { get; set; }
        public string meetingRoom { get; set; }
        public object meetingRoomId { get; set; }
        public object meetingRoomObj { get; set; }
        public object reviewRemarks { get; set; }
        public string meetingCategory { get; set; }
        public object projectId { get; set; }
        public object project { get; set; }
        public object categoryId { get; set; }
        public object category { get; set; }
        public string inviteEmailMethod { get; set; }
        public string ecoUrl { get; set; }
        public string createdBy { get; set; }
        public DateTime created { get; set; }
        public string lastModifiedBy { get; set; }
        public DateTime lastModified { get; set; }
        public bool isActive { get; set; }
    }

    public class Organizedby
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string designation { get; set; }
        public string mobileNumber { get; set; }
        public string officeNumber { get; set; }
        public string password { get; set; }
        public bool isTemporyPassword { get; set; }
        public string remark { get; set; }
        public string permissionLevel { get; set; }
        public object appUserDocuments { get; set; }
        public string userType { get; set; }
        public object company { get; set; }
        public string createdBy { get; set; }
        public DateTime created { get; set; }
        public string lastModifiedBy { get; set; }
        public DateTime lastModified { get; set; }
        public bool isActive { get; set; }
    }

    public class Meetingparticipant
    {
        public int id { get; set; }
        public int participantId { get; set; }
        public Participant participant { get; set; }
        public int meetingMasterId { get; set; }
        public string organizingPermission { get; set; }
        public object verifyParticipation { get; set; }
        public object verificationMethod { get; set; }
        public object verifiedTime { get; set; }
        public string invitationStatus { get; set; }
        public DateTime invitationStatusUpdatedDateTime { get; set; }
        public object reviewStatus { get; set; }
        public DateTime reviewCompletedDateTime { get; set; }
        public string createdBy { get; set; }
        public DateTime created { get; set; }
        public object lastModifiedBy { get; set; }
        public object lastModified { get; set; }
        public bool isActive { get; set; }
    }

    public class Participant
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string designation { get; set; }
        public string mobileNumber { get; set; }
        public string officeNumber { get; set; }
        public string password { get; set; }
        public bool isTemporyPassword { get; set; }
        public string remark { get; set; }
        public string permissionLevel { get; set; }
        public object appUserDocuments { get; set; }
        public string userType { get; set; }
        public object company { get; set; }
        public string createdBy { get; set; }
        public DateTime created { get; set; }
        public string lastModifiedBy { get; set; }
        public DateTime lastModified { get; set; }
        public bool isActive { get; set; }
    }

    public class Meetingagendaitem
    {
        public int id { get; set; }
        public string agendaItem { get; set; }
        public int agendaNumber { get; set; }
        public string type { get; set; }
        public object discussion { get; set; }
        public object conclusion { get; set; }
        public object isConfidentialAI { get; set; }
        public int meetingMasterId { get; set; }
        public object meetingActionItems { get; set; }
        public object meetingReviewerComments { get; set; }
        public string createdBy { get; set; }
        public DateTime created { get; set; }
        public object lastModifiedBy { get; set; }
        public object lastModified { get; set; }
        public bool isActive { get; set; }
    }

}
