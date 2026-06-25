const ADMIN_PASSWORD = 'change-me-before-deploy';
const gallery = document.querySelector('#gallery');
const filters = document.querySelector('#categoryFilters');
const formStatus = document.querySelector('#formStatus');
const adminStatus = document.querySelector('#adminStatus');
const adminPanel = document.querySelector('#adminPanel');
const adminArtworkList = document.querySelector('#adminArtworkList');

let artworks = [];
let activeCategory = 'Все';

document.querySelector('#year').textContent = new Date().getFullYear();

async function loadArtworks() {
  const response = await fetch('data/artworks.json');
  if (!response.ok) {
    throw new Error('Не удалось загрузить список работ');
  }

  artworks = (await response.json()).filter((item) => item.published);
  renderFilters();
  renderGallery();
  renderAdminPreview();
}

function renderFilters() {
  const categories = ['Все', ...new Set(artworks.map((item) => item.category))];
  filters.innerHTML = categories
    .map((category) => `<button class="filter${category === activeCategory ? ' filter--active' : ''}" type="button" data-category="${category}">${category}</button>`)
    .join('');
}

function renderGallery() {
  const visibleArtworks = activeCategory === 'Все' ? artworks : artworks.filter((item) => item.category === activeCategory);

  gallery.innerHTML = visibleArtworks
    .map(
      (item) => `
        <article class="card artwork">
          <img src="${item.image}" alt="${item.title}" loading="lazy" />
          <div class="artwork__body">
            <span>${item.category} · ${item.year}</span>
            <h3>${item.title}</h3>
            <p>${item.description}</p>
          </div>
        </article>
      `,
    )
    .join('');
}

function renderAdminPreview() {
  adminArtworkList.innerHTML = artworks.map((item) => `<li>${item.title} <span>${item.category}</span></li>`).join('');
}

filters.addEventListener('click', (event) => {
  const button = event.target.closest('button[data-category]');
  if (!button) return;

  activeCategory = button.dataset.category;
  renderFilters();
  renderGallery();
});

document.querySelector('#commissionForm').addEventListener('submit', (event) => {
  event.preventDefault();
  const formData = Object.fromEntries(new FormData(event.currentTarget));
  formStatus.textContent = `Заявка для «${formData.type}» подготовлена. Следующий этап — подключить отправку автору.`;
  event.currentTarget.reset();
});

document.querySelector('#adminLogin').addEventListener('submit', (event) => {
  event.preventDefault();
  const password = new FormData(event.currentTarget).get('password');

  // TODO(deploy-agent): replace this prototype-only guard with server-side authentication before production deploy.
  if (password !== ADMIN_PASSWORD) {
    adminStatus.textContent = 'Неверный пароль для прототипа.';
    return;
  }

  adminStatus.textContent = 'Вход выполнен. Показан черновик административного блока.';
  adminPanel.hidden = false;
  event.currentTarget.reset();
});

loadArtworks().catch((error) => {
  gallery.innerHTML = `<p class="error">${error.message}</p>`;
});
