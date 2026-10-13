<template>
  <div class="auth-surface">
    <header class="auth-nav">
      <NuxtLink to="/" class="auth-brand"><CommonBrandMark /><span>FastQuiz</span></NuxtLink>
      <span class="auth-caption">专注学习，从容应考。</span>
    </header>
    <main class="auth-body"><slot /></main>
    <footer class="auth-footer">FastQuiz · 让每一次学习，都有进步。</footer>
  </div>
</template>
