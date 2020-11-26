using AutoMapper;
using Code.Hub.Shared.Configurations.DevOps;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using Zammad.Client.Resources;

namespace Code.Hub.Core.Dtos
{
    public class CustomObjectMapper : Profile
    {
        public CustomObjectMapper()
        {
            AllowNullCollections = true;
            AllowNullDestinationValues = true;

            CreateMappings();
        }

        private void CreateMappings()
        {
            CreateMap<CreateOrEditUserInput, CodeHubUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)).ReverseMap();

            CreateMap<WorkItem, CodeHubWorkItem>()
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.AssignedTo) ? (src.Fields[StaticDevOpsWorkItemFields.AssignedTo] as IdentityRef).DisplayName : "Unassigned"))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.CreatedDate) ? src.Fields[StaticDevOpsWorkItemFields.CreatedDate] : new DateTime()))
                .ForMember(dest => dest.ChangedDate, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.ChangedDate) ? src.Fields[StaticDevOpsWorkItemFields.ChangedDate] : new DateTime()))
                .ForMember(dest => dest.WorkItemType, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.WorkItemType) ? src.Fields[StaticDevOpsWorkItemFields.WorkItemType] : "Unknown Work Item"))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.State) ? src.Fields[StaticDevOpsWorkItemFields.State] : "Unknown State"))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.Title) ? src.Fields[StaticDevOpsWorkItemFields.Title] : "No Title Provided"))
                .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.Fields.ContainsKey(StaticDevOpsWorkItemFields.TeamProject) ? src.Fields[StaticDevOpsWorkItemFields.TeamProject] : "No Project found"))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


            CreateMap<Ticket, CodeHubWorkItem>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt.DateTime))
                .ForMember(dest => dest.ChangedDate, opt => opt.MapFrom(src => src.UpdatedAt.DateTime))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }

    }
}
