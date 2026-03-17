using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace PetShop  // ВАЖНО: Замените YourProjectName на имя вашего проекта!
{
    /// <summary>
   
    /// Issue #1: Базовая генерация
    /// Issue #2: Шум (линии и точки)
    /// Issue #3: Наклон символов и спецэффекты
    /// </summary>
    public class CaptchaGenerator
    {
        private Random random = new Random();

        // Набор символов: цифры и латинские буквы (исключены похожие: O, 0, I, l, 1)
        private const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

        /// <summary>
        /// Генерирует случайный текст для CAPTCHA (4 символа)
        /// Issue #1
        /// </summary>
        public string GenerateCaptchaText(int length = 4)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// Создает изображение CAPTCHA со всеми эффектами
        /// Issue #1, #2, #3
        /// </summary>
        public Bitmap CreateCaptchaImage(string text, int width = 200, int height = 80)
        {
            // Создаем пустое изображение
            Bitmap bitmap = new Bitmap(width, height);

            // Graphics - это "кисть", которой мы рисуем
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // Настройки качества (делаем картинку красивее)
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                // Заливаем фон светло-серым
                graphics.Clear(Color.FromArgb(240, 240, 240));

                // Issue #2: Рисуем шум
                DrawNoiseLines(graphics, width, height);  // Рисуем линии
                DrawNoiseDots(bitmap, width, height);     // Рисуем точки

                // Issue #3: Рисуем искаженный текст
                DrawDistortedText(graphics, text, width, height);

                // Issue #3: Добавляем финальный эффект (перечеркивание ИЛИ наложение)
                AddSpecialEffect(graphics, width, height);
            }
            return bitmap;
        }

        /// <summary>
        /// Рисует случайные линии для создания шума
        /// Issue #2
        /// </summary>
        private void DrawNoiseLines(Graphics graphics, int width, int height)
        {
            using (Pen pen = new Pen(Color.FromArgb(100, 150, 150, 150))) // Полупрозрачный серый
            {
                for (int i = 0; i < 10; i++) // Рисуем 10 случайных линий
                {
                    int x1 = random.Next(width);
                    int y1 = random.Next(height);
                    int x2 = random.Next(width);
                    int y2 = random.Next(height);
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        /// <summary>
        /// Рисует случайные точки для создания шума
        /// Issue #2
        /// </summary>
        private void DrawNoiseDots(Bitmap bitmap, int width, int height)
        {
            for (int i = 0; i < 100; i++) // Рисуем 100 случайных точек
            {
                int x = random.Next(width);
                int y = random.Next(height);
                bitmap.SetPixel(x, y, Color.FromArgb(100, 100, 100));
            }
        }

        /// <summary>
        /// Рисует текст с искажениями (каждый символ повернут)
        /// Issue #3
        /// </summary>
        private void DrawDistortedText(Graphics graphics, string text, int width, int height)
        {
            float fontSize = height * 0.5f; // Размер шрифта = половина высоты картинки
            using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
            {
                // Начальная позиция X (с отступом 10 пикселей)
                float x = 10f;
                // Ширина одного символа
                float charWidth = (width - 20) / text.Length;

                for (int i = 0; i < text.Length; i++)
                {
                    // Сохраняем текущее состояние графики
                    GraphicsState state = graphics.Save();

                    // Сдвигаем "холст" к центру текущего символа
                    graphics.TranslateTransform(x + charWidth / 2, height / 2);

                    // Поворачиваем на случайный угол от -25 до 25 градусов
                    float angle = random.Next(-25, 26);
                    graphics.RotateTransform(angle);

                    // Выбираем случайный цвет для символа
                    Color charColor = Color.FromArgb(
                        random.Next(50, 200),
                        random.Next(50, 200),
                        random.Next(50, 200)
                    );

                    using (Brush brush = new SolidBrush(charColor))
                    {
                        // Рисуем символ
                        graphics.DrawString(text[i].ToString(), font, brush,
                                           -charWidth / 2, -fontSize / 2);
                    }

                    // Восстанавливаем состояние для следующего символа
                    graphics.Restore(state);

                    // Передвигаемся к следующему символу
                    x += charWidth;
                }
            }
        }

        /// <summary>
        /// Добавляет спецэффект: перечеркивание ИЛИ наложение
        /// Issue #3
        /// </summary>
        private void AddSpecialEffect(Graphics graphics, int width, int height)
        {
            // Случайно выбираем тип эффекта
            if (random.Next(2) == 0) // 0 - перечеркивание
            {
                using (Pen pen = new Pen(Color.FromArgb(128, 255, 0, 0), 3)) // Полупрозрачная красная линия
                {
                    // Диагональная линия слева-направо
                    graphics.DrawLine(pen, 0, 0, width, height);
                    // Диагональная линия справа-налево
                    graphics.DrawLine(pen, width, 0, 0, height);
                }
            }
            else // 1 - наложение (рисуем полупрозрачный текст поверх)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(50, 255, 0, 0))) // Красный 20% прозрачности
                using (Font font = new Font("Arial", 40, FontStyle.Bold))
                {
                    graphics.DrawString("CAPTCHA", font, brush, 20, 20);
                }
            }
        }
    }
}