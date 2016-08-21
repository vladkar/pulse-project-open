using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Common.Scenery.Objects;
using Pulse.MultiagentEngine.Map;

namespace NavigField
{
    public interface INavfieldCalc
    {
        /// <summary>
        /// Загружает входные данные
        /// </summary>
        /// <param name="toPulseVector2Multiplier">размер ячейки(метров в клетке)</param>
        /// <param name="obstacles">обстаклы</param>
        /// <param name="initPoints">(пока можно использовать только одну) точки, относительно которых выполняется обход ячеек, пригодных для перемещения</param>
        /// <param name="customBorder">(пока игнорировать) кастомная граница растеризации обстаклов</param>
        void Load(double toPulseVector2Multiplier, IList<PulseVector2[]> obstacles, PulseVector2[] initPoints, PulseVector2[] customBorder = null);

        /// <summary>
        /// рассчитывает поле для массива точек (пути)
        /// </summary>
        /// <param name="aimPoint"></param>
        /// <param name="path"></param>
        /// <param name="iterToPreciseAngles"></param>
        /// массив точек(путь)
        /// количество итераций для уточняющих итераций
        void CalculateField(PulseVector2 aimPoint, PulseVector2[] path, int iterToPreciseAngles);

        /// <summary>
        /// рассчитывает поле для одной точки
        /// </summary>
        /// <param name="aimPoint"></param>целевая точка
        /// <param name="iterToPreciseAngles"></param>количество итераций для уточняющих итераций
        /// <param name="preferredVelocity"></param>желаемая(максимальная) скорость агента        
        int CalcFieldForOneAim(PulseVector2 aimPoint, int iterToPreciseAngles, double preferredVelocity = 1D);

        /// <summary>
        /// возвращает вектор велосити для точки (точка задана в тех же координатах, что и обстаклы)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        PulseVector2 GetVelocity(PulseVector2 point);
    }

}
