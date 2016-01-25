﻿using GoodAI.Modules.School.LearningTasks;
using GoodAI.Modules.School.Worlds;

namespace GoodAI.Modules.School.Common
{
    public enum AbilityNameEnum
    {
        SimplestPatternDetection,
    }

    public enum LearningTaskNameEnum
    {
        DebuggingTask,
        DetectWhite,
        DetectBlackAndWhite,
        DetectShape,
        ApproachTarget,
        SimpleSizeDetection,
        SimpleDistanceDetection,
        MovingTarget,
        OneDApproach,
        DetectColor,
        CooldownAction,
        DetectAngle,
        DetectShapeColor,
        ShapeGroups,
        CopyAction,
        CopySequence,
        DetectDifference,
        DetectSimilarity
    }

    public class LearningTaskFactory
    {
        public static ILearningTask CreateLearningTask(LearningTaskNameEnum learningTaskName, AbstractSchoolWorld w)
        {
            // UAAAAAAAAAAA
            var world = w as ManInWorld; 

            switch (learningTaskName)
            {
                case LearningTaskNameEnum.DetectWhite:
                    return new LTDetectWhite(world);
                case LearningTaskNameEnum.ApproachTarget:
                    return new LTApproach(world);
                case LearningTaskNameEnum.SimpleSizeDetection:
                    return new LTSimpleSize(world);
                case LearningTaskNameEnum.SimpleDistanceDetection:
                    return new LTSimpleDistance(world);
                case LearningTaskNameEnum.DebuggingTask:
                    return new LTDebugging(world);
                case LearningTaskNameEnum.MovingTarget:
                    return new LTMovingTarget(world);
                case LearningTaskNameEnum.OneDApproach:
                    return new LT1DApproach(world);
                case LearningTaskNameEnum.DetectColor:
                    return new LTDetectColor(world);
                case LearningTaskNameEnum.CooldownAction:
                    return new LTActionWCooldown(world);
                case LearningTaskNameEnum.DetectShape:
                    return new LTDetectShape(world);
                case LearningTaskNameEnum.DetectShapeColor:
                    return new LTDetectShapeColor(world);
                case LearningTaskNameEnum.DetectBlackAndWhite:
                    return new LTDetectBlackAndWhite(world);
                case LearningTaskNameEnum.DetectAngle:
                    return new LTSimpleAngle(world);
                case LearningTaskNameEnum.ShapeGroups:
                    return new LTShapeGroups(world as RoguelikeWorld);
                case LearningTaskNameEnum.CopyAction:
                    return new LTCopyAction(world);
                case LearningTaskNameEnum.CopySequence:
                    return new LTCopySequence(world);
                case LearningTaskNameEnum.DetectDifference:
                    return new LTDetectDifference(world);
                case LearningTaskNameEnum.DetectSimilarity:
                    return new LTDetectSimilarity(world);
                default:
                    return null;
            }
        }
    }

}
