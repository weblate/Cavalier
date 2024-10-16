using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Gtk.CheckButton _waveCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _levelsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _particlesCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _barsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _spineCheckButton;
    [Gtk.Connect] private readonly Adw.ComboRow _mirrorRow;
    [Gtk.Connect] private readonly Gtk.Scale _marginScale;
    [Gtk.Connect] private readonly Adw.ComboRow _directionRow;
    [Gtk.Connect] private readonly Adw.ActionRow _offsetRow;
    [Gtk.Connect] private readonly Gtk.Scale _offsetScale;
    [Gtk.Connect] private readonly Adw.ActionRow _roundnessRow;
    [Gtk.Connect] private readonly Gtk.Scale _roundnessScale;
    [Gtk.Connect] private readonly Gtk.Switch _fillingSwitch;
    [Gtk.Connect] private readonly Adw.ActionRow _thicknessRow;
    [Gtk.Connect] private readonly Gtk.Scale _thicknessScale;
    [Gtk.Connect] private readonly Gtk.Switch _borderlessSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _sharpCornersSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _windowControlsSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _autohideHeaderSwitch;
    [Gtk.Connect] private readonly Adw.ComboRow _framerateRow;
    [Gtk.Connect] private readonly Gtk.Scale _barsScale;
    [Gtk.Connect] private readonly Gtk.Switch _autosensSwitch;
    [Gtk.Connect] private readonly Gtk.Scale _sensitivityScale;
    [Gtk.Connect] private readonly Gtk.ToggleButton _stereoButton;
    [Gtk.Connect] private readonly Gtk.Switch _monstercatSwitch;
    [Gtk.Connect] private readonly Gtk.Scale _noiseReductionScale;
    [Gtk.Connect] private readonly Gtk.Switch _reverseSwitch;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        _waveCheckButton.OnToggled += (sender, e) =>
        {
            if (_waveCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.WaveBox;
                _offsetRow.SetSensitive(false);
                _roundnessRow.SetSensitive(false);
            }
        };
        _levelsCheckButton.OnToggled += (sender, e) =>
        {
            if (_levelsCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.LevelsBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        _particlesCheckButton.OnToggled += (sender, e) =>
        {
            if (_particlesCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.ParticlesBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        _barsCheckButton.OnToggled += (sender, e) =>
        {
            if (_barsCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.BarsBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(false);
            }
        };
        _spineCheckButton.OnToggled += (sender, e) =>
        {
            if (_spineCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.SpineBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        switch (_controller.Mode)
        {
            case DrawingMode.WaveBox:
                _waveCheckButton.SetActive(true);
                break;
            case DrawingMode.LevelsBox:
                _levelsCheckButton.SetActive(true);
                break;
            case DrawingMode.ParticlesBox:
                _particlesCheckButton.SetActive(true);
                break;
            case DrawingMode.BarsBox:
                _barsCheckButton.SetActive(true);
                break;
            case DrawingMode.SpineBox:
                _spineCheckButton.SetActive(true);
                break;
        }
        if (_controller.Stereo)
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("Full"), _("Split Channels") }));
            _mirrorRow.SetSelected((uint)_controller.Mirror);
        }
        else
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("On") }));
            if (_controller.Mirror == Mirror.SplitChannels)
            {
                _mirrorRow.SetSelected(1u);
            }
            else
            {
                _mirrorRow.SetSelected((uint)_controller.Mirror);
            }
        }
        _mirrorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Mirror = (Mirror)_mirrorRow.GetSelected();
                _controller.Save();
            }
        };
        _marginScale.SetValue((int)_controller.AreaMargin);
        _marginScale.OnValueChanged += (sender, e) =>
        {
            _controller.AreaMargin = (uint)_marginScale.GetValue();
            _controller.ChangeWindowSettings();
        };
        _directionRow.SetSelected((uint)_controller.Direction);
        _directionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Direction = (DrawingDirection)_directionRow.GetSelected();
                _controller.Save();
            }
        };
        _offsetScale.SetValue((int)(_controller.ItemsOffset * 100));
        _offsetScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsOffset = (float)_offsetScale.GetValue() / 100.0f;
            _controller.Save();
        };
        _roundnessScale.SetValue((int)(_controller.ItemsRoundness * 100));
        _roundnessScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsRoundness = (float)_roundnessScale.GetValue() / 100.0f;
            _controller.Save();
        };
        _fillingSwitch.SetActive(_controller.Filling);
        _fillingSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Filling = _fillingSwitch.GetActive();
                _controller.Save();
            }
        };
        _thicknessRow.SetSensitive(!_fillingSwitch.GetActive());
        _thicknessScale.SetValue((int)_controller.LinesThickness);
        _thicknessScale.OnValueChanged += (sender, e) =>
        {
            _controller.LinesThickness = (float)_thicknessScale.GetValue();
            _controller.Save();
        };
        _borderlessSwitch.SetActive(_controller.Borderless);
        _borderlessSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Borderless = _borderlessSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _sharpCornersSwitch.SetActive(_controller.SharpCorners);
        _sharpCornersSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.SharpCorners = _sharpCornersSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _windowControlsSwitch.SetActive(_controller.ShowControls);
        _windowControlsSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ShowControls = _windowControlsSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _autohideHeaderSwitch.SetActive(_controller.AutohideHeader);
        _autohideHeaderSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.AutohideHeader = _autohideHeaderSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _framerateRow.SetSelected(_controller.Framerate / 30u - 1u);
        _framerateRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Framerate = (_framerateRow.GetSelected() + 1u) * 30u;
                _controller.ChangeCavaSettings();
            }
        };
        _barsScale.SetValue((int)_controller.BarPairs * 2);
        _barsScale.OnValueChanged += (sender, e) =>
        {
            if (_barsScale.GetValue() % 2 != 0)
            {
                _barsScale.SetValue(_barsScale.GetValue() - 1);
                return;
            }
            _controller.BarPairs = (uint)(_barsScale.GetValue() / 2);
            _controller.ChangeCavaSettings();
        };
        _autosensSwitch.SetActive(_controller.Autosens);
        _autosensSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Autosens = _autosensSwitch.GetActive();
                _controller.ChangeCavaSettings();
            }
        };
        _sensitivityScale.SetValue((int)_controller.Sensitivity);
        _sensitivityScale.OnValueChanged += (sender, e) =>
        {
            _controller.Sensitivity = (uint)_sensitivityScale.GetValue();
            _controller.ChangeCavaSettings();
        };
        _stereoButton.SetActive(_controller.Stereo);
        _stereoButton.OnToggled += (sender, e) =>
        {
            if (_stereoButton.GetActive())
            {
                _controller.Stereo = true;
                _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("Full"), _("Split Channels") }));
            }
            else
            {
                _controller.Stereo = false;
                _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("On") }));
            }
            _controller.ChangeCavaSettings();
        };
        _monstercatSwitch.SetActive(_controller.Monstercat);
        _monstercatSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Monstercat = _monstercatSwitch.GetActive();
                _controller.ChangeCavaSettings();
            }
        };
        _noiseReductionScale.SetValue((double)_controller.NoiseReduction);
        _noiseReductionScale.GetFirstChild().SetMarginBottom(12);
        _noiseReductionScale.AddMark(0.77, Gtk.PositionType.Bottom, null);
        _noiseReductionScale.OnValueChanged += (sender, e) =>
        {
            _controller.NoiseReduction = (float)_noiseReductionScale.GetValue();
            _controller.ChangeCavaSettings();
        };
        _reverseSwitch.SetActive(_controller.ReverseOrder);
        _reverseSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ReverseOrder = _reverseSwitch.GetActive();
                _controller.Save();
            }
        };
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : this(Builder.FromFile("preferences_dialog.ui"), controller, application, parent)
    {
    }
}
