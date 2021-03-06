﻿using GoodAI.Modules.School.Worlds;
using System.ComponentModel;

namespace GoodAI.Modules.School.LearningTasks.TransducerTasks
{
    [DisplayName("Single pixel - Difficulty 3.1 RL")]
    public class LTSinglePixelTransducerGrayDisruptingBlackAndWhite : LTSinglePixelTransducerRL
    {

        public LTSinglePixelTransducerGrayDisruptingBlackAndWhite() : this(null) { }

        public LTSinglePixelTransducerGrayDisruptingBlackAndWhite(SchoolWorld w)
            : base(w)
        {

        }

        public override void CreateTransducer()
        {
            // this automaton provides rewards if the agent correctly identifies strings of black or white symbols; 
            // if there's a gray symbol, there have to be 1-2 black symbols afterwards, depending on occurence of white symbols.
            m_ft = new FiniteTransducer(3, 3, 2);

            m_ft.SetInitialState(0);

            m_ft.AddFinalState(0);

            m_ft.AddTransition(0, 0, 0, 1);
            m_ft.AddTransition(0, 0, 1, 1);
            m_ft.AddTransition(0, 1, 2, 0);
            m_ft.AddTransition(1, 2, 0, 0);
            m_ft.AddTransition(1, 0, 1, 0);
            m_ft.AddTransition(1, 1, 2, 0);
            m_ft.AddTransition(2, 2, 0, 0);
            m_ft.AddTransition(2, 1, 1, 0);
            m_ft.AddTransition(2, 1, 2, 0);

            m_importantActions.Add(1);
        }
    }
}