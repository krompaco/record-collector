import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
  static targets = ['toggleable', 'button']
  static values = {
    open: { type: Boolean, default: false },
    openText: String,
    closeText: String,
  }

  initialize() {
    this.toggleClass = 'hidden';
    this.isConnected = false;
  }

  connect() {
    document.body.addEventListener("keydown", this._onBodyKeydown);

    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', 'false');
      target.setAttribute('aria-controls', 'header-menu');
      target.querySelector('.js-text-content').textContent = this.openTextValue;
    });

    this.toggleableTargets.forEach(target => {
      target.classList.add(this.toggleClass);
    });

    this.isConnected = true;
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
    if (!this.isConnected) { return }

    if (!this.toggleClass) { return }

    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', this.openValue);
      target.setAttribute('aria-label', this.openValue ? this.closeTextValue : this.openTextValue);

      // Hamburger graphics
      target.querySelector('.js-content-expanded').classList.toggle(this.toggleClass);
      target.querySelector('.js-content-collapsed').classList.toggle(this.toggleClass);
    });

    this.toggleableTargets.forEach(target => {
      target.classList.toggle(this.toggleClass);
    });
  }
}
