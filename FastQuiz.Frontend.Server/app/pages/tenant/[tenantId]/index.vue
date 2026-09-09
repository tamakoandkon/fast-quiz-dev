<template>
  <div>
    <main class="content">
      <div class="placeholder-container">
        <h1 class="logo-text">FastQuiz</h1>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts"></script>

<style scoped>
.content {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}

.placeholder-container {
  display: flex;
  justify-content: center;
  align-items: center;
}

.logo-text {
  font-size: 3rem;
  color: #d1d5db; /* 灰度色调 */
  font-weight: bold;
  user-select: none;
}
</style>
