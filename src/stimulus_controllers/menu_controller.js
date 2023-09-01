import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
  static targets = ['toggleable', 'button']
  static values = {
    openText: String,
    closeText: String,
  }

  initialize() {
    this.toggleClass = 'hidden';
    this.isOpen = false;
  }

  connect() {
    document.body.addEventListener("keydown", this._onBodyKeydown);
    this.setMenuState(false);
  }

  disconnect() {
    document.body.removeEventListener("keydown", this._onBodyKeydown);

    this.isOpen = false;
    this.setMenuState(false);
  }

  toggle(event) {
    event.preventDefault();

    this.isOpen = !this.isOpen;
    this.setMenuState(this.isOpen);
  }

  _onBodyKeydown = event => {
    if (event.key === 'Escape') {
      this.isOpen = false;
      this.setMenuState(false);
    }
  }

  setMenuState(open) {
    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', open);
      target.setAttribute('aria-label', open ? this.closeTextValue : this.openTextValue);
      target.querySelector('.js-text-content').textContent = open ? this.closeTextValue : this.openTextValue;

      // Hamburger graphics
      if (open) {
        target.querySelector('.js-content-expanded').classList.remove(this.toggleClass);
        target.querySelector('.js-content-collapsed').classList.add(this.toggleClass);
      }
      else {
        target.querySelector('.js-content-expanded').classList.add(this.toggleClass);
        target.querySelector('.js-content-collapsed').classList.remove(this.toggleClass);
      }
    });

    this.toggleableTargets.forEach(target => {
      if (open) {
        target.classList.remove(this.toggleClass);
      }
      else {
        target.classList.add(this.toggleClass);
      }
    });
  }
}
