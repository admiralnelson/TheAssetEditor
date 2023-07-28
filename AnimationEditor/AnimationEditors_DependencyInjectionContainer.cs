﻿using AnimationEditor.AnimationTransferTool;
using AnimationEditor.CampaignAnimationCreator;
using AnimationEditor.Common.AnimationPlayer;
using AnimationEditor.Common.BaseControl;
using AnimationEditor.Common.ReferenceModel;
using AnimationEditor.MountAnimationCreator;
using AnimationEditor.PropCreator.ViewModels;
using AnimationEditor.SkeletonEditor;
using AnimationEditor.SuperView;
using CommonControls;
using CommonControls.Services.ToolCreation;
using Microsoft.Extensions.DependencyInjection;

namespace AnimationEditor
{
    public class AnimationEditors_DependencyInjectionContainer : DependencyContainer
    {
        public override void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<SceneObjectBuilder>();
            serviceCollection.AddTransient<SceneObject>();
            serviceCollection.AddScoped<AnimationPlayerViewModel>();
            serviceCollection.AddScoped<SceneObjectViewModelBuilder>();

            serviceCollection.AddScoped<MountAnimationCreator.Editor>();
            serviceCollection.AddScoped<AnimationTransferTool.Editor>();

            serviceCollection.AddScoped<MountAnimationCreatorViewModel>();
            serviceCollection.AddScoped<AnimationTransferToolViewModel>();
          
            serviceCollection.AddScoped<SkeletonEditorViewModel>();
            serviceCollection.AddScoped<BaseAnimationView>();

            serviceCollection.AddScoped<EditorHost<SuperViewViewModel>>();
            serviceCollection.AddScoped<SuperViewViewModel>();


            serviceCollection.AddScoped<EditorHost<SkeletonEditorViewModel>>();
            serviceCollection.AddScoped<SkeletonEditorViewModel>();

            serviceCollection.AddScoped<EditorHost<CampaignAnimationCreatorViewModel>>();
            serviceCollection.AddScoped<CampaignAnimationCreatorViewModel>();
        }

        public override void RegisterTools(IToolFactory factory)
        {
            factory.RegisterTool<MountAnimationCreatorViewModel, BaseAnimationView>();
            factory.RegisterTool<AnimationTransferToolViewModel, BaseAnimationView>();
            factory.RegisterTool<EditorHost<SuperViewViewModel>, BaseAnimationView>();
            factory.RegisterTool<EditorHost<SkeletonEditorViewModel>, BaseAnimationView>();
            factory.RegisterTool<EditorHost<CampaignAnimationCreatorViewModel>, BaseAnimationView>();
        }
    }
}
