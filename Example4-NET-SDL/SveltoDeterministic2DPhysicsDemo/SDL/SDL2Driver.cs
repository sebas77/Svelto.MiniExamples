using System;
using System.Collections.Generic;
using SDL2;
using MiniExamples.DeterministicPhysicDemo.Graphics.Extensions;

namespace MiniExamples.DeterministicPhysicDemo.Graphics
{
    public class Sdl2Driver : IGraphics, IInput
    {
        const int WindowWidth  = 1024;
        const int WindowHeight = 768;

        public void Cleanup()
        {
            if (_renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(_renderer);
                _renderer = IntPtr.Zero;
            }

            if (_window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(_window);
                _window = IntPtr.Zero;
            }

            SDL.SDL_Quit();
        }

        public void DrawBox(Colour colour, int minX, int minY, int maxX, int maxY)
        {
            minX = (int) Math.Round(minX * _zoom) + _cameraX;
            minY = (int) Math.Round(minY * _zoom) + _cameraY;
            maxX = (int) Math.Round(maxX * _zoom) + _cameraX;
            maxY = (int) Math.Round(maxY * _zoom) + _cameraY;

            var rgb = colour.ToRgb();
            SDL.SDL_SetRenderDrawColor(_renderer, rgb[0], rgb[1], rgb[2], 255);

            SDL.SDL_RenderDrawLine(_renderer, minX, minY, maxX, minY);
            SDL.SDL_RenderDrawLine(_renderer, maxX, minY, maxX, maxY);
            SDL.SDL_RenderDrawLine(_renderer, minX, maxY, maxX, maxY);
            SDL.SDL_RenderDrawLine(_renderer, minX, maxY, minX, minY);
        }

        public void DrawCircle(Colour colour, int centreX, int centreY, int radius)
        {
            centreX = (int) Math.Round(centreX * _zoom) + _cameraX;
            centreY = (int) Math.Round(centreY * _zoom) + _cameraY;
            radius  = (int) Math.Round(radius * _zoom);

            var rgb = colour.ToRgb();
            SDL.SDL_SetRenderDrawColor(_renderer, rgb[0], rgb[1], rgb[2], 255);

            var diameter = radius * 2;

            var x     = radius - 1;
            var y     = 0;
            var tx    = 1;
            var ty    = 1;
            var error = tx - diameter;

            while (x >= y)
            {
                //  Each of the following renders an octant of the circle
                SDL.SDL_RenderDrawPoint(_renderer, centreX + x, centreY - y);
                SDL.SDL_RenderDrawPoint(_renderer, centreX + x, centreY + y);
                SDL.SDL_RenderDrawPoint(_renderer, centreX - x, centreY - y);
                SDL.SDL_RenderDrawPoint(_renderer, centreX - x, centreY + y);
                SDL.SDL_RenderDrawPoint(_renderer, centreX + y, centreY - x);
                SDL.SDL_RenderDrawPoint(_renderer, centreX + y, centreY + x);
                SDL.SDL_RenderDrawPoint(_renderer, centreX - y, centreY - x);
                SDL.SDL_RenderDrawPoint(_renderer, centreX - y, centreY + x);

                if (error <= 0)
                {
                    ++y;
                    error += ty;
                    ty    += 2;
                }

                if (error > 0)
                {
                    --x;
                    tx    += 2;
                    error += tx - diameter;
                }
            }
        }

        public void DrawCross(Colour colour, int centreX, int centreY, int radius)
        {
            centreX = (int) Math.Round(centreX * _zoom) + _cameraX;
            centreY = (int) Math.Round(centreY * _zoom) + _cameraY;
            radius  = (int) Math.Round(radius * _zoom);

            var rgb = colour.ToRgb();
            SDL.SDL_SetRenderDrawColor(_renderer, rgb[0], rgb[1], rgb[2], 255);

            SDL.SDL_RenderDrawLine(_renderer, centreX - radius, centreY - radius, centreX + radius, centreY + radius);
            SDL.SDL_RenderDrawLine(_renderer, centreX - radius, centreY + radius, centreX + radius, centreY - radius);
        }

        public void DrawLine(Colour colour, int fromX, int fromY, int toX, int toY)
        {
            fromX = (int) Math.Round(fromX * _zoom) + _cameraX;
            fromY = (int) Math.Round(fromY * _zoom) + _cameraY;
            toX   = (int) Math.Round(toX * _zoom) + _cameraX;
            toY   = (int) Math.Round(toY * _zoom) + _cameraY;

            var rgb = colour.ToRgb();
            SDL.SDL_SetRenderDrawColor(_renderer, rgb[0], rgb[1], rgb[2], 255);

            SDL.SDL_RenderDrawLine(_renderer, fromX, fromY, toX, toY);
        }

        public void DrawPlus(Colour colour, int centreX, int centreY, int radius)
        {
            centreX = (int) Math.Round(centreX * _zoom) + _cameraX;
            centreY = (int) Math.Round(centreY * _zoom) + _cameraY;
            radius  = (int) Math.Round(radius * _zoom);

            var rgb = colour.ToRgb();
            SDL.SDL_SetRenderDrawColor(_renderer, rgb[0], rgb[1], rgb[2], 255);

            SDL.SDL_RenderDrawLine(_renderer, centreX - radius, centreY, centreX + radius, centreY);
            SDL.SDL_RenderDrawLine(_renderer, centreX, centreY - radius, centreX, centreY + radius);
        }

        public void DrawText(Colour color, int x, int y, string text, int ptSize = 12)
        {
            x = (int) Math.Round(x * _zoom) + _cameraX;
            y = (int) Math.Round(y * _zoom) + _cameraY;

            DrawTextAbsolute(color, x, y, text, ptSize);
        }

        public void DrawTextAbsolute(Colour color, int x, int y, string text, int ptSize = 12)
        {
            var rgb  = color.ToRgb();
            var font = GetFont(ptSize);

            var surfaceMessage = SDL_ttf.TTF_RenderText_Solid(font, text, new SDL.SDL_Color
            {
                r = rgb[0]
              , g = rgb[1]
              , b = rgb[2]
              , a = 255
            });

            if (surfaceMessage == IntPtr.Zero)
            {
                Console.WriteLine("unable to render text");
                return;
            }

            SDL_ttf.TTF_SizeText(font, text, out var w, out var h);
            var message = SDL.SDL_CreateTextureFromSurface(_renderer, surfaceMessage);

            var rectMessage = new SDL.SDL_Rect
            {
                x = x
              , y = y
              , w = w
              , h = h
            };
            SDL.SDL_RenderCopy(_renderer, message, IntPtr.Zero, ref rectMessage);

            SDL.SDL_DestroyTexture(message);
            SDL.SDL_FreeSurface(surfaceMessage);
        }

        public static int GetWindowHeight() { return WindowHeight; }

        public static int GetWindowWidth() { return WindowWidth; }

        public bool Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
                return false;

            if (SDL_ttf.TTF_Init() < 0)
                return false;

            if (SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1") == SDL.SDL_bool.SDL_FALSE)
                Console.WriteLine("SDL_HINT_RENDER_SCALE_QUALITY failed");

            if ((_window = SDL.SDL_CreateWindow("SDL2", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED
                                              , WindowWidth, WindowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN))
             == IntPtr.Zero)
            {
                Console.WriteLine("unable to create window");
                Cleanup();
                return false;
            }

            _primarySurface = SDL.SDL_GetWindowSurface(_window);

            if ((_renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED))
             == IntPtr.Zero)
            {
                Console.WriteLine("unable to create renderer");
                Cleanup();
                return false;
            }

            GetFont(12);
            SDL.SDL_SetRenderDrawColor(_renderer, 0xae, 0xFF, 0x00, 0xFF);

            return true;
        }

        public void OnEvent(SDL.SDL_Event @event, IGameLoop gameLoop)
        {
            if (!@event.type.Equals(SDL.SDL_EventType.SDL_KEYDOWN))
                return;

            if (@event.key.keysym.sym.Equals(SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                Console.WriteLine("Escape pressed.");
                gameLoop.Stop();
            }

            float? simulationSpeed = @event.key.keysym.sym switch
            {
                SDL.SDL_Keycode.SDLK_BACKQUOTE => 10.0f, SDL.SDL_Keycode.SDLK_1 => 1.0f, SDL.SDL_Keycode.SDLK_2 => 0.5f
              , SDL.SDL_Keycode.SDLK_3 => 0.25f, SDL.SDL_Keycode.SDLK_4 => 0.125f, SDL.SDL_Keycode.SDLK_5 => 0.01f
              , SDL.SDL_Keycode.SDLK_6 => 0.0f, _ => null
            };

            if (simulationSpeed.HasValue)
            {
                Console.WriteLine($"Set simulation speed to {simulationSpeed.Value}");
                gameLoop.SetSimulationSpeed(simulationSpeed.Value);
            }

            _cameraX += @event.key.keysym.sym switch
            {
                SDL.SDL_Keycode.SDLK_LEFT => 10, SDL.SDL_Keycode.SDLK_RIGHT => -10, _ => 0
            };

            _cameraY += @event.key.keysym.sym switch
            {
                SDL.SDL_Keycode.SDLK_UP => 10, SDL.SDL_Keycode.SDLK_DOWN => -10, _ => 0
            };

            _zoom += @event.key.keysym.sym switch
            {
                SDL.SDL_Keycode.SDLK_HOME => 0.25f, SDL.SDL_Keycode.SDLK_END => -0.25f, _ => 0.0f
            };

            _zoom = Math.Max(_zoom, 0.25f);
        }

        public void PollEvents(IGameLoop gameLoop)
        {
            while (SDL.SDL_PollEvent(out var @event) != 0)
            {
                OnEvent(@event, gameLoop);
                if (@event.type == SDL.SDL_EventType.SDL_QUIT)
                    gameLoop.Stop();
            }
        }

        public void RenderEnd()
        {
            SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 0);
            SDL.SDL_RenderPresent(_renderer);
        }

        public void RenderStart()
        {
            SDL.SDL_RenderClear(_renderer);

            DrawPlus(Colour.Gray, 0, 0, 5000);
        }

        IntPtr GetFont(int ptSize)
        {
            if (_fontPoints.ContainsKey(ptSize))
                return _fontPoints[ptSize];

            IntPtr fontPtr;
            if ((fontPtr = SDL_ttf.TTF_OpenFont("OpenSans-Light.ttf", ptSize)) == IntPtr.Zero)
                throw new Exception("unable to load font");
            return _fontPoints[ptSize] = fontPtr;
        }

        readonly Dictionary<int, IntPtr> _fontPoints = new Dictionary<int, IntPtr>();
        int                              _cameraX    = WindowWidth / 2;
        int                              _cameraY    = WindowHeight / 2;
        IntPtr                           _primarySurface;
        IntPtr                           _renderer = IntPtr.Zero;
        IntPtr                           _window   = IntPtr.Zero;

        float _zoom = 2.0f;
    }
}