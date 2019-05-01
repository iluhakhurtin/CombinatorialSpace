using Concepts.Concepts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.TextConcepts
{
    /// <summary>
    /// This class represents one letter in Alexey's Redozubov article https://habr.com/en/post/326334/ 
    /// as a part of concept system.
    /// 
    /// Введем систему понятий, позволяющую описать все, что появляется в скользящем окне. 
    /// Для простоты не будем делать разницы между заглавными и строчными буквами и не будем учитывать знаки препинания.   
    /// Так как потенциально любая буква может встретиться нам в любой позиции, 
    /// то нам понадобятся все возможные сочетания букв и позиций, то есть 26 x K понятий. 
    /// Условно эти понятия можно обозначить как
    /// {a0,a1⋯a(K-1),b0,b1⋯b(K-1),z0,z1⋯z(K-1)}
    /// Мы можем записать любое состояние скользящего окна как перечисление соответствующих понятий.
    /// Так для окна, показанного на рисунке выше, это будет
    /// {s1,w3,e4,d6}
    /// Сопоставим каждому понятию некий разряженный бинарный код.
    /// Например, если взять длину кода в 256 бит и отвести 8 бит для кодирования одного понятия, 
    /// то кодирование буквы «a» в различных позициях будет выглядеть как показано на рисунке ниже.
    /// Напомню, что коды выбираются случайным образом, и биты у них могут повторяться, 
    /// то есть одни и те же биты могут быть общими для нескольких кодов.
    /// </summary>
    class CharConceptItem : IConceptItem<byte, char>
    {
        #region Properties 

        /// <summary>
        /// It is position of a symbol in the article.
        /// </summary>
        public byte Key { get; set; }

        /// <summary>
        /// It is symbol in the article.
        /// </summary>
        public char Value { get; set; }

        /// <summary>
        /// This is a random bit array reprepresenting the symbol.
        /// </summary>
        public BitArray Vector { get; set; }

        #endregion

        #region Constructors

        public CharConceptItem()
        {
        }

        public CharConceptItem(char value, BitArray vector)
            : this(value, vector, 0)
        {
            
        }

        public CharConceptItem(char value, BitArray vector, byte key)
        {
            this.Value = value;
            this.Vector = vector;
            this.Key = key;
        }

        #endregion
    }
}
