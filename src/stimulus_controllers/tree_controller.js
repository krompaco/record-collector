import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
  static targets = ['button']

  initialize() {
    this.toggleClass = 'hidden';
    this.isOpen = false;
  }

  connect() {
    this.setMenuState(false);
  }

  disconnect() {
    this.isOpen = false;
    this.setMenuState(false);
  }

  toggle(event) {
    event.preventDefault();
    this.isOpen = !this.isOpen;
    this.setMenuState(this.isOpen);
  }

  setMenuState(open) {
    this.buttonTargets.forEach(target => {
      var listId = target.getAttribute('data-aria-controls');
      target.setAttribute('aria-controls', listId);
      target.setAttribute('aria-expanded', open);

      if (open) {
        document.getElementById(listId).classList.remove(this.toggleClass);
      }
      else {
        document.getElementById(listId).classList.add(this.toggleClass);
      }

      // SVGs
      if (open) {
        target.querySelector('.js-content-expanded').classList.remove(this.toggleClass);
        target.querySelector('.js-content-collapsed').classList.add(this.toggleClass);
      }
      else {
        target.querySelector('.js-content-expanded').classList.add(this.toggleClass);
        target.querySelector('.js-content-collapsed').classList.remove(this.toggleClass);
      }
    })
  }
}
