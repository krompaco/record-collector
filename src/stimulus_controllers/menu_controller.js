import { Controller } from 'stimulus'

export default class extends Controller {
  static targets = ['toggleable', 'button']
  static values = { open: Boolean }

  initialize() {
    this.toggleClass = 'hidden';
    this.openText = 'Open menu';
  }

  connect() {
    document.body.addEventListener("keydown", this._onBodyKeydown);

    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', 'false');
      target.setAttribute('aria-controls', 'header-menu');
      target.querySelector('.js-text-content').textContent = this.openText;
    });
  }

  toggle(event) {
    event.preventDefault();
    this.openValue = !this.openValue;
  }

  _onBodyKeydown = event => {
    if (event.key === 'Escape') {
      this.openValue = false;
    }
  }

  openValueChanged() {
    if (!this.toggleClass) { return }

    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', this.openValue);
      target.setAttribute('aria-label', this.openValue ? 'Close menu' : this.openText);

      // Hamburger graphics
      target.querySelector('.js-content-expanded').classList.toggle(this.toggleClass);
      target.querySelector('.js-content-collapsed').classList.toggle(this.toggleClass);
    });

    this.toggleableTargets.forEach(target => {
      target.classList.toggle(this.toggleClass);
    });
  }
}
